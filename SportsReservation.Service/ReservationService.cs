using AutoMapper;
using Newtonsoft.Json;
using RabbitMQ.Client;
using SportsReservation.Core.Abstract;
using SportsReservation.Core.Abstract.Services;
using SportsReservation.Core.Models;
using SportsReservation.Core.Models.DTO_S;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsReservation.Service
{
    public class ReservationService:IReservationService
    {
        private readonly IGenericRepository<Reservation> _reservationRepository;
        private readonly IReservationRepository _reservationNonGenericRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ReservationService(IGenericRepository<Reservation> reservationRepository, IReservationRepository reservationNonGenericRepo, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _reservationRepository = reservationRepository;
            _reservationNonGenericRepo = reservationNonGenericRepo;
            _unitOfWork = unitOfWork;
            _mapper = mapper;   
        }
        public async Task<Response<ReservationDto>> CreateReservationAsync(ReservationDto reservationDto, string userRole)
        {
            if (reservationDto.StartTime.DayOfWeek == DayOfWeek.Sunday)
            {
                return Response<ReservationDto>.Fail(400, new List<string> { "Pazar günü rezervasyon yapılamaz." });
            }
            if (reservationDto.StartTime.Hour < 9)
            {
                return Response<ReservationDto>.Fail(400, new List<string> { "Rezervasyon sabah en erken 09:00'da başlayabilir." });
            }
            if (userRole != "Yönetici" && (reservationDto.EndTime - reservationDto.StartTime).TotalHours > 1)
            {
                return Response<ReservationDto>.Fail(400, new List<string> { "Öğrenci ve personel yalnızca 1 saatlik rezervasyon yapabilir." });
            }
            if (userRole == "Yönetici" && (reservationDto.EndTime - reservationDto.StartTime).TotalMinutes % 60 != 0)
            {
                return Response<ReservationDto>.Fail(400, new List<string> { "Yönetici sadece 1 saat veya katları şeklinde rezervasyon yapabilir." });
            }
            var existingReservations = await _reservationRepository.GetAllAsync();
            bool hasExistingReservation = existingReservations.Any(r => r.UserId == reservationDto.UserId &&
                r.CreateDate >= DateTime.UtcNow.Date.AddDays(-((int)DateTime.UtcNow.DayOfWeek)));

            if (hasExistingReservation)
            {
                throw new InvalidOperationException("User can only have one reservation per week.");
            }

            bool isOverlapping = existingReservations.Any(r => r.CreateDate == reservationDto.CreateDate &&
                ((r.StartTime <= reservationDto.StartTime && r.EndTime > reservationDto.StartTime) ||
                 (r.StartTime < reservationDto.EndTime && r.EndTime >= reservationDto.EndTime)));

            if (isOverlapping)
            {
                throw new InvalidOperationException("Selected time slot is already reserved.");
            }
            Reservation reservation = _mapper.Map<Reservation>(reservationDto);
            await _reservationRepository.CreateAsync(reservation);
            await _unitOfWork.CommitAsync();


            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new("amqps://qjgqxgdo:m_h6n4U-1ZKF9RnrBOOwbyJU91y21A14@rattlesnake.rmq.cloudamqp.com/qjgqxgdo");
            using IConnection connection = factory.CreateConnection();
            using IModel channel = connection.CreateModel();
            channel.ExchangeDeclare(exchange: "directly-exchange", type: ExchangeType.Direct,durable:true);
            channel.QueueDeclare(queue: "UnPaidReservationQueue", durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(queue: "UnPaidReservationQueue", exchange: "directly-exchange", routingKey: "UnPaidReservation");
            //Bu kuyruğu direkt bir dinleyici ile tüketmediğimiz için kuyruğu ve exchangi mesajın kaybolma ihtimali göz önüne alınarak durable yaptım
            //Çünkü kullanıcı 2 gün sonra ödeme yapmak isteyebilir ve durable olmadığı durumda kuyruk bulunamayabilir.
            string message = JsonConvert.SerializeObject(reservation);
            byte[] byteMessage = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange:"directly-exchange",routingKey:"UnPaidReservation",body: byteMessage);
            return Response<ReservationDto>.Success(reservationDto, 200); 
        }
        public async Task<Response<NoDataDto>> CancelUnpaidReservationsAsync()
        {
            var reservations = await _reservationRepository.GetAllAsync();
            var now = DateTime.UtcNow;

            var unpaidReservations = reservations.Where(r => !r.IsPaid && (r.StartTime - now.Date).TotalDays <= 1).ToList();

            if (!unpaidReservations.Any())
            {
                return Response<NoDataDto>.Fail(400, new List<string> {"Ödenmemiş rezervasyon bulunamadı"}); 
            }

            ConnectionFactory factory = new ConnectionFactory
            {
                Uri = new Uri("amqps://qjgqxgdo:m_h6n4U-1ZKF9RnrBOOwbyJU91y21A14@rattlesnake.rmq.cloudamqp.com/qjgqxgdo")
            };

            using IConnection connection = factory.CreateConnection();
            using IModel channel = connection.CreateModel();

            string queueName = "UnPaidReservationQueue";
            string exchangeName = "directly-exchange";
            string routingKey = "UnPaidReservation";

            channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct, durable: true);
            channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);

            foreach (var reservation in unpaidReservations)
            {
                // ✅ Önce RabbitMQ Kuyruğundaki al
                BasicGetResult result = channel.BasicGet(queueName, false);
                if (result != null)
                {
                    string message = Encoding.UTF8.GetString(result.Body.Span);
                    var reservationData = JsonConvert.DeserializeObject<Reservation>(message);

                    if (reservationData != null && reservationData.Id == reservation.Id)
                    {
                        // RabbitMQ'dan ilgili rezervasyonu kaldır
                        //Normal şartlarda BasicGetResult result = channel.BasicGet(queueName, true); olsaydı burdaki true mesajı otomatik sildirirdi.
                        //Ancak işlem başarısız olursa mesaj kaybı olmasın diye false çektim ve bunu BasicAck ile kuyruktan silme yöntemine çevirdim.
                        channel.BasicAck(result.DeliveryTag, false);
                    }
                }
                _reservationRepository.Delete(reservation);
            }

            await _unitOfWork.CommitAsync();
            return Response<NoDataDto>.Fail(200, new List<string> { "Ödenmemiş rezervasyonlar kaldırıldı" });
        }

        public async Task<Response<List<ReservationInfoDto>>> GetAllReservation()
        {
            var reservations = await _reservationRepository.GetAllAsync();
            var reservationDtos = _mapper.Map<List<ReservationInfoDto>>(reservations);
            return Response<List<ReservationInfoDto>>.Success(reservationDtos, 200);
        }
        public async Task<Response<ReservationInfoWithPaidInfo>> GetMyReservation(Guid userId)
        {
            var myReservation = await _reservationNonGenericRepo.GetByGuidIdAsync(userId);
            return Response<ReservationInfoWithPaidInfo>.Success(myReservation, 200);
        }

        public async Task<Response<NoDataDto>> DeleteMyReservationsAsync(Guid userId)
        {
            var reservationNonGeneric = await _reservationNonGenericRepo.DeleteByGuidIdAsync(userId);
            await _unitOfWork.CommitAsync();
            return Response<NoDataDto>.Fail(200, new List<string> { "Rezervasyon silindi." });
        }
    }
}
