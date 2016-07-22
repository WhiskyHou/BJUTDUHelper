using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BJUTDUHelper.Manager
{
    public class AccountManager
    {
        public static DAL.LocalSetting _localSetting = new DAL.LocalSetting();
        public static async Task<T> GetAccount<T>()where T:class
        {
            return await _localSetting.GetLocalInfo<T>(typeof(T).Name);
        }
        public static async void SetAccount<T>(T entity) where T : class
        {
            await _localSetting.SetLocalInfo<T>(entity.GetType().Name, entity);
        }
    }
}
