namespace SoftwareQualityPrediction.Utils
{
    public interface IColleague
    {
        IMediator Mediator { get; set; }
        void Send(object message);
        void Receive(object message);
    }
}
