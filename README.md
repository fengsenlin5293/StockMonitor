## 传送门   [Github源码下载](https://github.com/fengsenlin5293/StockMonitor)
***爬虫配置：*** 包括 *股票数量/线程*、*每次查询数量*、*查询时间间隔* 三个配置项。</br>
配置项|详细描述|备注
--|--|--
## 1.引言
正如本文标题所述，这是一款基于股票数据爬虫并且可以自定义策略的实时股票监控程序。此程序是本人业余时间所写，必然存在一些考虑不全面的或者有瑕疵的地方，欢迎大家指出和交流。
目前的版本我只写了两种策略，如果有感兴趣的朋友可以去Github下载源码，拓展自己想要的策略。
**另外需要声明的是：**
**1.大家请勿用此软件作为投资依据,否则后果自负。**
**2.本人写这款软件仅供技术交流,请勿用于商业及非法用途,如产生法律纠纷与本人无关。**
## 2.为什么写这款软件
因为目前股票市场上存在不少股民是根据‘‘盘口异动’’来做股票交易的。而目前很多行情软件的盘口信息都有一定滞后的。比如，通过盘口信息发现某只股票突然快速的上涨了6-7个点，而此时你再去买入已无利润可言，一般都是想在第一时间发现它还有上涨空间，比如在它上涨1-2个点的时候就发现了它。
以我身边朋友为例，他是用的东方财富证券，属于右侧交易者，习惯于追涨。根据盘口异动信息来做交易，经常出现买入的价格是接近于当天的最高价。
比如2019年1月21日，东方财富的盘口信息，如下图（朗迪集团）

![东方财富-盘口信息](https://upload-images.jianshu.io/upload_images/11337307-25d8e2d46b4549e3.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)
![东方财富分时-朗迪集团](https://upload-images.jianshu.io/upload_images/11337307-43a9972575a18f93.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)
如果在10:39分看到盘口信息朗迪集团快速上涨了就立刻买入，这就买在了当天的最高点。
如果能在前面的10:03-10:06分的那波拉升就发现它，那到收盘会有些许盈利。
可东方财富行情客户端盘口信息并未有此提示，这应该跟算法有关。他们的算法我无从知晓，我能够知道的是个股的实时成交数据，有了这些数据为什么不能自己写算法呢？于是产生了写这样一款软件的想法。
## 3.软件架构
![软件架构图](https://upload-images.jianshu.io/upload_images/11337307-4776d4435d3cebec.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)
## 4.软件一览
### 4.1.监控页面
![主页](https://upload-images.jianshu.io/upload_images/11337307-23b8432be88eaeed.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)
### 4.2.基本设置
***代理设置：*** 如果勾上，软件所有的http请求都将使用这个代理来发起请求。![代理设置](https://upload-images.jianshu.io/upload_images/11337307-9c8e59cfc3fb1d0a.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)
***监控范围：*** 可以配置一些条件，筛选想要监控的股票。也可以把自己关注的股票放到文本文件里面导入进来。![监控范围](https://upload-images.jianshu.io/upload_images/11337307-108a7af0f976847e.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)
***不想监控的股票：*** 如果有不想监控的股票，可以放到某个文本文件，导入进来。
![不想监控的股票](https://upload-images.jianshu.io/upload_images/11337307-8b30063841623224.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)
***策略设置：*** 选择监控的策略。（目前我就写了两种策略）
![策略设置](https://upload-images.jianshu.io/upload_images/11337307-aa6f08eaa430a468.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)
### 4.3.高级设置
***爬虫配置：*** 包括 *股票数量/线程*、*每次查询数量*、*查询时间间隔* 三个配置项。</br>
配置项|详细描述|备注
--|--|--
股票数量/线程|每个数据爬虫线程负责的股票数|100-300 
每次查询数量|每只股票每次查询的成交记录的数量|20-14400
查询时间间隔|每一轮查询的时间间隔|大于3000毫秒
<br>
![爬虫配置](https://upload-images.jianshu.io/upload_images/11337307-17dfc0bbed8832c4.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)
***策略一：大单策略***
配置项|详细描述|备注
--|--|--
每组个股数|每个数据分析线程负责一个组，每组的容量|100-500 
缓存处理线程数|缓存数据分组及处理使用的线程数量|1-3
向前截取|某只股票的某条成交记录向前选取的时间段|10-300秒
向后截取|某只股票的某条成交记录向后选取的时间段|0-60秒
大单金额|设定一个值表示成交金额达到此值才算是大单|大于10万元
大单个数要求|在选取的范围内要求大单的个数|大于0
![大单策略](https://upload-images.jianshu.io/upload_images/11337307-82972ee8938c6499.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)

***策略二：快速上涨策略***
配置项|详细描述|备注<br>
--|--|--<br>
每组个股数|每个数据分析线程负责一个组，每组的容量|100-500 <br>
缓存处理线程数|缓存数据分组及处理使用的线程数量|1-3<br>
向前截取|某只股票的某条成交记录向前选取的时间段|10-300秒<br>
向后截取|某只股票的某条成交记录向后选取的时间段|0-60秒<br>
金额大于|快速上涨策略的前提条件：某只股票某笔交易金额达到多少|至少大于5万元<br>
快速涨幅大于|设定一个值表示上涨幅度达到此值才算是快速上涨|0.5个点-10个点<br>
![快速上涨策略](https://upload-images.jianshu.io/upload_images/11337307-d1d9dfe348e4e77b.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)

### 4.4.数据同步
“**个股基本数据**”每天至少需要同步一次数据，因为每天股票的开盘价、收盘价、停牌、复牌等数据都是不一样的。<br>
“**题材个股关系数据**”看个人需求来更新。
![同步数据](https://upload-images.jianshu.io/upload_images/11337307-bff758852a54a5e9.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)

### 4.5.关于
![关于](https://upload-images.jianshu.io/upload_images/11337307-dbe86ee9e607e850.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)

## 5.软件局限性
### 5.1.成交数据受限于数据请求接口，
**东方财富网** 接口数据更新延迟20-50s不定
**新浪网** 接口数据更新延迟相对稳定30s左右

*注：这项测试是两个接口都是1s请求一次，如果数据与上一次请求有不一致，则打印到控制台*
![东方财富网与新浪网接口数据返回对比图](https://upload-images.jianshu.io/upload_images/11337307-46e6f450dd26fa5d.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)
这是最大的局限性，不管是爬取东方财富网还是新浪网的成交数据，软件拿到的成交数据相比于用行情软件看到的实时成交数据有一定延迟。
## 6.缺点
### 6.1.CPU占用较高
![cpu占用](https://upload-images.jianshu.io/upload_images/11337307-d633e2986ab2ffc4.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)![本机处理器](https://upload-images.jianshu.io/upload_images/11337307-7cd3e233dc32508d.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)
针对CPU占用高的问题，本人尝试过使用
```System.Diagnostics.Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)0x0003;```
这种方式来指定爬虫线程用哪个cpu，分析线程用哪个cpu，ui线程用哪个cpu。
结果是，这种方式可以降低cpu占用，但是数据分析处理能力降低许多，影响了时效性。对于实时监控类程序来说时效性尤为重要，因此暂时放弃使用这种方式解决cpu占用问题。
## 7.优点
### 7.1拓展策略
如果感兴趣的朋友可自己拓展自己定制的策略。
1.新策略请继承并实现下面接口，算法根据自己需求写。
```
    public interface IAnalysisStrategy<TStockBase>
    {
        event StockAnalysisResultUpdatedEventHandler<TStockBase> AnalysisResultUpdatedEvent;
        event EventHandler<int> RemainderCountUpdatedEvent;
        void Start(IEnumerable<TStockBase> stockBases);
        void Stop();
    }
```
2.使用策略（以大单策略为例）
定义字段 `_stockAnalysisService`
```
IStockAnalysisService<IAnalysisStrategy<StockTransactionModel>, StockTransactionModel> _stockAnalysisService;
```
订阅事件/开始分析
```
var stockAnalysisService = ServiceManager<StockTransactionModel>.Instance.GetStockAnalysisService();
_stockAnalysisService = stockAnalysisService;
_stockAnalysisService.StockAnalysisResultUpdatedEvent += StockAnalysisService_StockAnalysisResultUpdatedEvent;
_stockAnalysisService.RemainderCountUpdatedEvent += _stockAnalysisService_RemainderCountUpdatedEvent;
_stockAnalysisService.StartAnalysis(new BigDealAnalysisStrategy(), _cacheBigDealAnalysisStrategy);
```
## 8.其他
### 8.1 中英文
如果习惯使用英文界面请更改程序配置文件，如下图，把value的值由“zh-CN”改为‘‘en-US’’
![配置文件名称](https://upload-images.jianshu.io/upload_images/11337307-956a9d047f472b9b.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)![配置文件内容](https://upload-images.jianshu.io/upload_images/11337307-77185cbfd49142e8.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)
## 结束语
**如果有幸有朋友简单使用了或深入研究了这个软件，有什么疑问或者建议都可以提出来，大家互相交流学习，共同成长。**
>联系方式：fengsenlin5293@163.com

>源码下载：[https://github.com/fengsenlin5293/StockMonitor](https://github.com/fengsenlin5293/StockMonitor)
