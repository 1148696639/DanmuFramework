
    using QFramework;
    namespace DMFramework
    {
        public class GameHttpUrlBaseQuery:AbstractQuery<string>
        {
            protected override string OnDo()
            {
                return this.GetModel<IGameConfigModel>().HttpUrlBase;
            }
        }
    }