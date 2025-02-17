using System;
using System.Threading.Tasks;
using TessModel;

namespace StepAPIService
{
    internal class Tessellator : IStepTessellator
    {
        private readonly TessProcessDispatcher tessProcessDispatcher;

        public Tessellator(TessProcessDispatcher tessProcessDispatcher)
        {
            this.tessProcessDispatcher = tessProcessDispatcher;
        }

        public async Task<Tessellation?> TessellateModel(string fileInput, long priority = 0)
        {
            var options = new ProgramOptions(fileInput);
            
            return await ExecuteJsonResultProcessAsync<Tessellation>(options, priority).ConfigureAwait(false);
        }

        private async Task<T?> ExecuteJsonResultProcessAsync<T>(ProgramOptions opt, long priority)
        {
            try
            {
                if (await tessProcessDispatcher
                        .ExecuteAsync<ProcessRequest, int>(ProcessRequest.CreateInstance(opt), priority)
                        .ConfigureAwait(false) == 0)
                {
                    var data = tessProcessDispatcher.GetProcessResponseForFileName(opt.InputFile);
                    return data != null ? ModelSerializer.Deserialize<T>(data) : default;
                }
            }
            catch (Exception ex)
            {
            }
            return default;
        }
    }
}
