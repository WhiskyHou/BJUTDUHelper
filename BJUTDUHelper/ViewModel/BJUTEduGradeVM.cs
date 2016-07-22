
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace BJUTDUHelper.ViewModel
{
    public class BJUTEduGradeVM:ViewModelBase
    {
        private readonly string messageToken = "1";
        private Service.HttpBaseService _httpService;
        private Model.GradeChart _gradeChart;
        public Model.GradeChart GradeChart
        {
            get { return _gradeChart; }
            set { Set(ref _gradeChart, value); }
        }
        public BJUTEduGradeVM()
        {
            //LoadedCommand = new RelayCommand<object>(Loaded);
        }
        //public ICommand LoadedCommand { get; set; }
        public async void Loaded(object parms)
        {
            //加载离线数据
            LoadGradeChart();

            if (parms != null)
            {
                _httpService = parms as Service.HttpBaseService;
                var user = await Manager.AccountManager.GetAccount<Model.BJUTEduCenterUserinfo>();
                
                GetGrade(ViewModel.BJUTEduCenterVM.Name, user.Username);//获取最新数据
            }
          
        }
        //public ICommand NavigateToCommand { get; set; }
        //public async void NavigateTo(object o)
        //{
        //    _httpService = o as Service.HttpBaseService;
        //    var user = await Manager.AccountManager.GetAccount<Model.BJUTEduCenterUserinfo>();
        //    GradeChart = new Model.GradeChart();
        //    GetGrade(user.Username, user.Password);
        //}

        #region 成绩本地管理逻辑
        public async void SaveGradeChart()
        {
            DAL.LocalSetting _localSetting = new DAL.LocalSetting();
            await _localSetting.SetLocalInfo<Model.GradeChart>(typeof(Model.GradeChart).Name, GradeChart);
        }

        private object lockObject = new object();
        public async void LoadGradeChart()
        {
            DAL.LocalSetting _localSetting = new DAL.LocalSetting();
            var chart=await _localSetting.GetLocalInfo<Model.GradeChart>(typeof(Model.GradeChart).Name);
            lock (lockObject)
            {
                GradeChart = chart;
            }
            GradeChart?.InitList();
        }
        #endregion

        #region 学年学期列表逻辑
        private string _weightScore;
        public string WeightScore
        {
            get { return _weightScore; }
            set { Set(ref _weightScore, value); }
        }

        private int _selectedTermIndex = -1;
        public int SelectedTermIndex
        {
            get { return _selectedTermIndex; }
            set { Set(ref _selectedTermIndex, value); }
        }

        private string _selectedYear;
        public string SelectedYear
        {
            get { return _selectedYear; }
            set { Set(ref _selectedYear, value); }
        }

        public void cbSchoolYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           SelectedTermIndex = -1;
           GradeChart.GetSpecificGradeChart(SelectedYear, SelectedTermIndex.ToString());
            //lvGrade.ItemsSource = gd.gc;
            WeightScore = GradeChart.GetWeightAvarageScore();
        }
        public void cbTerm_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GradeChart.GetSpecificGradeChart(SelectedYear, (SelectedTermIndex+1).ToString());

            WeightScore = GradeChart.GetWeightAvarageScore();
        }
        #endregion

        #region 工大教务系统页面解析逻辑
        private async void GetGrade(string name, string username)
        {
            try
            {
                string re;
                re = await _httpService.SendRequst("http://gdjwgl.bjut.edu.cn/xscjcx.aspx?xh=" + username + "&xm=" + name + "&gnmkdm=N121605", HttpMethod.Get, referUri: "http://gdjwgl.bjut.edu.cn/xscjcx.aspx?xh=" + username + "&xm=" + "" + "&gnmkdm=N121605");


                string __VIEWSTATEString;
                __VIEWSTATEString =Service.BJUTEduCenterService.GetViewstate(re);

                IDictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("__EVENTTARGET", "");
                parameters.Add("__EVENTARGUMENT", "");
                parameters.Add("__VIEWSTATE", __VIEWSTATEString);
                parameters.Add("hidLanguage", "");
                parameters.Add("ddlXN", "");
                parameters.Add("ddlXQ", "");
                parameters.Add("ddl_kcxz", "");
                parameters.Add("btn_zcj", "历年成绩");

                re = await _httpService.SendRequst("http://gdjwgl.bjut.edu.cn/xscjcx.aspx?xh=" + username + "&xm=" + "" + "&gnmkdm=N121605", HttpMethod.Post, parameters);

                if (GradeChart == null)
                {
                    GradeChart = new Model.GradeChart();
                }
                GradeChart.GetGradeChart(re);//解析成绩表
                SaveGradeChart();//保存成绩表

            }
            catch (Exception ex)
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send("解析成绩出错", messageToken);
            }
        }
        #endregion
    }
}
