using QFramework;
using NotImplementedException = System.NotImplementedException;

namespace DMFramework
{
    public class GameExitIsRegisterQuery:AbstractQuery<bool>
    {
        protected override bool OnDo()
        {
            return this.GetModel<IGameConfigModel>().GameExitIsRegister;
        }
    }
}