using CommonHelpers;
using GalaSoft.MvvmLight;
using StockHelpers;
using Structures.Attibutes;
using Structures.Stocks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Stock
{
    public class StockModel : ObservableObject
    {
        public static int StaticIndex = 1;
        public StockModel()
        {
            _topicModels = new ObservableCollection<TopicModel>();
            Index = StaticIndex++;
        }

        public StockModel(StockTransactionModel model) : this()
        {
            SetData(model);
            this.TypeString = ResourceHelper.FindKey("Monitor_TypeString_BigDeal");
        }
        public StockModel(StockTransactionModelExtern model) : this()
        {
            SetData(model);
            this.QuickUpPercent = model.QuickUpPercent;
            this.TypeString = ResourceHelper.FindKey("Monitor_TypeString_QuickUp");
        }

        private void SetData(StockTransactionModel model)
        {
            if (model != null)
            {
                this.Code = model.Code;
                this.Name = model.Name;
                this.CurrentPercent = model.CurrentPercent;
                this.Time = model.Time;
                this.Price = model.Price;
                this.DealHands = model.DealHands;
                this.DealType = model.DealType;
                this.Status = model.Status;
                this.DealHands2 = model.DealHands2;
                this.Unknow2 = model.Unknow2;
                this.Unknow3 = model.Unknow3;
                this.CurrentTime = DateTime.Now.ToString("HH:mm:ss");
                if (!string.IsNullOrEmpty(model.Topic))
                {
                    var topics = model.Topic.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var topic in topics)
                    {
                        var url = CommonStockDataManager.Instance.GetStorageSectionUrl(topic);
                        TopicModel temp = new TopicModel();
                        temp.Topic = topic;
                        temp.UrlStr = url;
                        this.TopicModels.Add(temp);
                    }
                }
            }
        }
        #region 属性

        private int _index;
        [ColumnDescription("Monitor_TableHeader_Index", nameof(Index))]
        public int Index
        {
            get { return _index; }
            set { Set(() => Index, ref _index, value); }
        }

        private string _typeString;
        [ColumnDescription("Monitor_TableHeader_TypeString", nameof(TypeString))]
        public string TypeString
        {
            get { return _typeString; }
            set { Set(() => TypeString, ref _typeString, value); }
        }



        private string _code;
        [ColumnDescription("Monitor_TableHeader_Code", nameof(Code))]
        public string Code
        {
            get { return _code; }
            set { Set(() => Code, ref _code, value); }
        }

        private string _name;
        [ColumnDescription("Monitor_TableHeader_Name", nameof(Name))]
        public string Name
        {
            get { return _name; }
            set { Set(() => Name, ref _name, value); }
        }


        private double _currentPercent = 0.0;
        [ColumnDescription("Monitor_TableHeader_CurrentPercent", nameof(CurrentPercent))]
        public double CurrentPercent
        {
            get { return _currentPercent; }
            set { Set(() => CurrentPercent, ref _currentPercent, value); }
        }

        private string _time;
        [ColumnDescription("Monitor_TableHeader_Time", nameof(Time))]
        public string Time
        {
            get { return _time; }
            set { Set(() => Time, ref _time, value); }
        }

        private string _currentTime;
        [ColumnDescription("Monitor_TableHeader_CurrentTime", nameof(CurrentTime))]
        public string CurrentTime
        {
            get { return _currentTime; }
            set { Set(() => CurrentTime, ref _currentTime, value); }
        }

        private double _price;
        [ColumnDescription("Monitor_TableHeader_Price", nameof(Price))]
        public double Price
        {
            get { return _price; }
            set { Set(() => Price, ref _price, value); }
        }

        private double _dealHands;
        [ColumnDescription("Monitor_TableHeader_DealHands", nameof(DealHands))]
        public double DealHands
        {
            get { return _dealHands; }
            set { Set(() => DealHands, ref _dealHands, value); }
        }

        private BuyOrSale _dealType;
        [ColumnDescription("Monitor_TableHeader_DealType", nameof(DealType))]
        public BuyOrSale DealType
        {
            get { return _dealType; }
            set { Set(() => DealType, ref _dealType, value); }
        }

        private StockStatus _status;
        [ColumnDescription("Monitor_TableHeader_Status", nameof(Status))]
        public StockStatus Status
        {
            get { return _status; }
            set { Set(() => Status, ref _status, value); }
        }

        private double _dealHands2;
        //[ColumnDescription("DealHands2", nameof(DealHands2))]
        public double DealHands2
        {
            get { return _dealHands2; }
            set { Set(() => DealHands2, ref _dealHands2, value); }
        }

        private int _unknow2;
        //[ColumnDescription("Unknow2", nameof(Unknow2))]
        public int Unknow2
        {
            get { return _unknow2; }
            set { Set(() => Unknow2, ref _unknow2, value); }
        }

        private int _unknow3;
        //[ColumnDescription("Unknow3", nameof(Unknow3))]
        public int Unknow3
        {
            get { return _unknow3; }
            set { Set(() => Unknow3, ref _unknow3, value); }
        }


        private double _quickUpPercent;
        [ColumnDescription("Monitor_TableHeader_QuickUpPercent", nameof(QuickUpPercent))]
        public double QuickUpPercent
        {
            get { return _quickUpPercent; }
            set { Set(() => QuickUpPercent, ref _quickUpPercent, value); }
        }

        private ObservableCollection<TopicModel> _topicModels;
        [ColumnCollectionAttribute()]
        public ObservableCollection<TopicModel> TopicModels
        {
            get { return _topicModels; }
            set { Set(() => TopicModels, ref _topicModels, value); }
        }



        #endregion
    }
}
