namespace SoftwareQualityPrediction.Utils
{
    public interface IMediator
    {
        void Send(object message, IColleague colleague);
    }
}
