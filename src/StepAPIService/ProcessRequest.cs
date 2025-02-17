using MediatR;

namespace StepAPIService
{
    internal class ProcessRequest : IRequest<int>
    {
        private ProcessRequest()
        {
        }

        public ProgramOptions? Options { get; private set; }

        public static ProcessRequest CreateInstance(ProgramOptions opt) => new () { Options = opt };
    }
}
