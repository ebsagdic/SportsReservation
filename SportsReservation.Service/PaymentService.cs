using AutoMapper;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.AspNet.Identity;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;



//using Microsoft.Extensions.Options;
using SportsReservation.Core.Abstract;
using SportsReservation.Core.Abstract.Services;
using SportsReservation.Core.Models;
using SportsReservation.Core.Models.DTO_S;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace SportsReservation.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IGenericRepository<Reservation> _reservationRepository;
        private readonly IGenericRepository<PaymentModel> _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IyzicOptions _iyzicOptions;
        public PaymentService(Microsoft.Extensions.Options.IOptions<IyzicOptions> configuration,IGenericRepository<Reservation> reservationRepository,IGenericRepository<PaymentModel> paymentRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _reservationRepository = reservationRepository;
            _paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _iyzicOptions = configuration.Value;
        }
        public async Task<Response<PaymentModel>> PaymentIyzico(PaymentDto paymentDto, int amount)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            //Aslında using kullanamdan da yapılabilir çünkü BeginTransactionAsync in geldiği interface IDbContextTransaction iDisposable ,
            //ancak transactionu manuel dispose etmekle uğraşmayalım diye bu yapıyı tercih ettim.

            Options options = new Options();
            options.ApiKey = _iyzicOptions.ApiKey;
            options.SecretKey = _iyzicOptions.SecurityKey;
            options.BaseUrl = "https://sandbox-api.iyzipay.com";

            CreatePaymentRequest request = new CreatePaymentRequest();
            request.Locale = Locale.TR.ToString();
            request.ConversationId = Guid.NewGuid().ToString();
            request.Price = amount.ToString();
            request.PaidPrice = amount.ToString();
            request.Currency = Currency.TRY.ToString();
            request.Installment = 1;
            request.PaymentChannel = PaymentChannel.WEB.ToString();
            request.PaymentGroup = PaymentGroup.PRODUCT.ToString();

            PaymentCard paymentCard = new PaymentCard();
            paymentCard.CardHolderName = paymentDto.CardHolderName ;
            paymentCard.CardNumber = paymentDto.CardNumber;
            paymentCard.ExpireMonth = paymentDto.ExpireMonth;
            paymentCard.ExpireYear = paymentDto.ExpireYear;
            paymentCard.Cvc = paymentDto.Cvc;
            paymentCard.RegisterCard = 0;
            request.PaymentCard = paymentCard;

            Buyer buyer = new Buyer();
            buyer.Id = "BY789";
            buyer.Name = "John";
            buyer.Surname = "Doe";
            buyer.GsmNumber = "+905350000000";
            buyer.Email = "email@email.com";
            buyer.IdentityNumber = "74300864791";
            buyer.LastLoginDate = "2015-10-05 12:43:35";
            buyer.RegistrationDate = "2013-04-21 15:12:09";
            buyer.RegistrationAddress = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
            buyer.Ip = "85.34.78.112";
            buyer.City = "Istanbul";
            buyer.Country = "Turkey";
            buyer.ZipCode = "34732";
            request.Buyer = buyer;

            Address shippingAddress = new Address();
            shippingAddress.ContactName = "Jane Doe";
            shippingAddress.City = "Istanbul";
            shippingAddress.Country = "Turkey";
            shippingAddress.Description = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
            shippingAddress.ZipCode = "34742";
            request.ShippingAddress = shippingAddress;

            Address billingAddress = new Address();
            billingAddress.ContactName = "Jane Doe";
            billingAddress.City = "Istanbul";
            billingAddress.Country = "Turkey";
            billingAddress.Description = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
            billingAddress.ZipCode = "34742";
            request.BillingAddress = billingAddress;

            List<BasketItem> basketItems = new List<BasketItem>();
            BasketItem firstBasketItem = new BasketItem
            {
                Id = "BI101",
                Name = "Product",
                Category1 = "Category",
                Category2 = "SubCategory",
                ItemType = BasketItemType.PHYSICAL.ToString(),
                Price = amount.ToString() 
            };
            basketItems.Add(firstBasketItem);

            request.BasketItems = basketItems;

            Payment iyzicoPayment = await Payment.Create(request, options);

            if (iyzicoPayment.Status == "success")
            {
                ConnectionFactory connectionFactory = new ConnectionFactory();
                connectionFactory.Uri = new Uri("amqps://qjgqxgdo:m_h6n4U-1ZKF9RnrBOOwbyJU91y21A14@rattlesnake.rmq.cloudamqp.com/qjgqxgdo");

                using IConnection connection = connectionFactory.CreateConnection();
                using IModel channel = connection.CreateModel();

                string queueName = "UnPaidReservationQueue";

                channel.ExchangeDeclare(exchange: "directly-exchange", type: ExchangeType.Direct, durable: true);

                channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);

                channel.QueueBind(queue: queueName,
                    exchange: "directly-exchange",
                    routingKey: "UnPaidReservation");

                BasicGetResult result = channel.BasicGet(queueName, true);

                if (result != null)
                {
                    string message = Encoding.UTF8.GetString(result.Body.Span);
                    var reservationData = JsonSerializer.Deserialize<Reservation>(message);


                    if (reservationData != null)
                    {
                        reservationData.IsPaid = true;
                        _reservationRepository.Update(reservationData);
                        await _unitOfWork.CommitAsync();

                        var paymentRecord = new PaymentModel
                        {
                            ReservationId = reservationData.Id,
                            Amount = amount,
                            PaymentStatus = true,
                            PaymentDate = DateTime.UtcNow,
                            CreateDate = DateTime.UtcNow,
                            CreateUser = reservationData.UserId.ToString(),
                        };

                        await _paymentRepository.CreateAsync(paymentRecord);
                        await _unitOfWork.CommitAsync();

                        channel.BasicAck(result.DeliveryTag, false);

                        await transaction.CommitAsync();

                        return Response<PaymentModel>.Success(paymentRecord, 200);
                    }
                }

                CreateRefundRequest refundRequest = new CreateRefundRequest
                {
                    Locale = Locale.TR.ToString(),
                    ConversationId = request.ConversationId,
                    PaymentTransactionId = iyzicoPayment.PaymentId,
                    Price = amount.ToString(),
                    Currency = Currency.TRY.ToString()
                };

                Refund refund = await Refund.Create(refundRequest, options);

                if (refund.Status == "success")
                {
                    await transaction.RollbackAsync(); 
                    return Response<PaymentModel>.Fail(404, new List<string> { "Rezervasyon bulunamadı, ödeme geri alındı." });
                }
                else
                {
                    await transaction.RollbackAsync(); 
                    return Response<PaymentModel>.Fail(500, new List<string> { "Ödeme iptal edilemedi! Manuel kontrol gerekli." });
                }
            }
            await transaction.RollbackAsync();
            var error = new List<string> { new string(iyzicoPayment.ErrorMessage.ToArray()) }; ;
            return Response<PaymentModel>.Fail(400, error);
        }
    }
}
