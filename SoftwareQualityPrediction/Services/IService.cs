using System;

namespace SoftwareQualityPrediction.Services
{
    public interface IService
    {
        IService Succesor { get; set; }
        Action OnCompleteCallback { get; set; }
        void Start(int? noOfSubsets = null, int? testingSubsetIndex = null);
    }
}
