using System.Threading.Tasks;

namespace TessModel
{
    public interface IStepTessellator
    {
        public Task<Tessellation?> TessellateModel(string fileInput, long priority = 0);
    }
}