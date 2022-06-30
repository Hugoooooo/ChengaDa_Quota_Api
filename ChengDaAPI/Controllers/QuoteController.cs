using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChengDaApi.Controllers.Job;
using ChengDaApi.DBRepositories.DBSchema;
using ChengDaApi.DBRepositories.IRepositories;
using DinkToPdf;
using DinkToPdf.Contracts;
using Domain;
using Domain.Models;
using Domain.Models.Quote;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ChengDaApi.Controllers
{
    [ApiController]
    [Route("QuoteService")]
    public class QuoteController : ControllerBase
    {

        private readonly ILogger<QuoteController> _logger;
        private readonly IDatabase _database;
        private readonly IConverter _pdfConverter;
        private readonly IConfiguration _config;
        private readonly IDapperr _dapper;
        private readonly IQuoteRepository _quoteRepository;
        private readonly IQuoteDetailRepository _quoteDetailRepository;
        private readonly ISystemParameterRepository _systemParameterRepository;



        public QuoteController(ILogger<QuoteController> logger, IConverter pdfConverter, IConfiguration configuration, IDapperr dapper, IDatabase database,
            IQuoteRepository quoteRepository, IQuoteDetailRepository quoteDetailRepository,ISystemParameterRepository systemParameterRepository)
        {
            _database = database;
            _logger = logger;
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _dapper = dapper;
            _pdfConverter = pdfConverter;
            _quoteRepository = quoteRepository;
            _quoteDetailRepository = quoteDetailRepository;
            _systemParameterRepository = systemParameterRepository;
        }

        [HttpGet]
        [Route("GetQuote")]
        public async Task<GetQuoteRes> GetQuote(string caseNumber)
        {
            GetQuoteRes res = new GetQuoteRes()
            {
                message = GenericResType.QUERY_SUCCESS
            };

            try
            {
                var quote = await _quoteRepository.GetByCase(caseNumber);
                if (quote != null)
                {
                    var details = await _quoteDetailRepository.GetByQId(quote.id);
                    res.project_name = quote.project_name;
                    res.customer_name = quote.customer_name;
                    res.customer_address = quote.customer_address;
                    res.contact_name = quote.contact_name;
                    res.phone = quote.phone;
                    res.mobile = quote.mobile;
                    res.companyTax = quote.companyTax;
                    res.fax = quote.fax;
                    res.create_user = quote.create_user;
                    res.fax_type = quote.fax_type;
                    res.items = details.Select(p =>
                    {
                        return new QuoteModel()
                        {
                            product = p.product,
                            quantity = p.quantity,
                            pattern = p.pattern,
                            unit_price = p.unit_price,
                            amount = p.amount,
                            remark = p.remark,
                            idx = p.idx
                        };
                    }).ToList();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(GenericResType.QUERY_FAIL + " ex:" + ex.ToString());

                res.isError = true;
                res.message = GenericResType.QUERY_FAIL;
            }

            return res;
        }

        [HttpPost]
        [Route("Preview")]
        public Stream Preview(ExportReq req)
        {
            try
            {
                string cellPhone = req.create_user == "劉居政" ? "0932-227-943" : "0932-121-776";
                #region 項目
                int totalAmount = 0; //總價
                int taxAmount = 0;  //稅額
                int rows = 20;
                string rowAssemble = string.Empty;

                for (int i = 1; i <= rows; i++)
                {
                    if (req.items != null && i <= req.items.Count)
                    {
                        var tmp = req.items[i - 1];
                        rowAssemble += $@"
                        <tr>
                            <td style='padding: 0 0.5em; border: 1px solid; height: 1.5em; text-align: center;'>{i}</td>
                            <td style='padding: 0 0.5em; border: 1px solid; height: 1.5em;'>{tmp.product}</td>
                            <td style='padding: 0 0.5em; border: 1px solid; height: 1.5em; text-align: center;'>{tmp.pattern}</td>
                            <td style='padding: 0 0.5em; border: 1px solid; height: 1.5em; text-align: center;'>{(tmp.quantity == 0 ? "" : tmp.quantity.ToString("#,0"))}</td>
                            <td style='padding: 0 0.5em; border: 1px solid; height: 1.5em; text-align: right;'>{(tmp.unit_price == 0 ? "" : tmp.unit_price.ToString("#,0"))}</td>
                            <td style='padding: 0 0.5em; border: 1px solid; height: 1.5em; text-align: right;'>{(tmp.amount == 0 ? "" : tmp.amount.ToString("#,0"))}</td>
                            <td style='padding: 0 0.5em; border: 1px solid; height: 1.5em; word-break: break-all;'>{tmp.remark}</td>
                        </tr>";
                        totalAmount += tmp.amount;
                    }
                    else
                    {
                        rowAssemble += $@"
                        <tr>
                            <td style='padding: 0 0.5em; border: 1px solid; height: 1.5em; text-align: center;'></td>
                            <td style='padding: 0 0.5em; border: 1px solid; height: 1.5em;'></td>
                            <td style='padding: 0 0.5em; border: 1px solid; height: 1.5em; text-align: center;'></td>
                            <td style='padding: 0 0.5em; border: 1px solid; height: 1.5em; text-align: center;'></td>
                            <td style='padding: 0 0.5em; border: 1px solid; height: 1.5em; text-align: right;'></td>
                            <td style='padding: 0 0.5em; border: 1px solid; height: 1.5em; text-align: right;'></td>
                            <td style='padding: 0 0.5em; border: 1px solid; height: 1.5em; word-break: break-all;'></td>
                        </tr>";
                    }

                }

                taxAmount = (int)Math.Round(totalAmount * 0.05, 0, MidpointRounding.AwayFromZero);
                #endregion

                int afterTaxTotal = 0;
                string totalTemplate = "";
                // 1: 外加  2: 內含
                if (req.fax_type == "1" || req.fax_type == "2")
                {
                    afterTaxTotal = req.fax_type == "1" ? (totalAmount + taxAmount) : totalAmount;
                    totalTemplate = $@"
                        <p style='margin: 0.5em 0;'>
                            <span style='width: 2em; display: inline-block; text-align-last: justify;'>NT$</span>：
                            <span style='width: 7em; display: inline-block; text-align: right;'>{totalAmount.ToString("#,0")}</span> 元
                        </p>
                        <p style='margin: 0.5em 0;'>
                            <span style='width: 2em; display: inline-block; text-align-last: justify;'>稅額</span>：
                            <span style='width: 7em; display: inline-block; border-bottom: 1px solid; text-align: right;'>{taxAmount.ToString("#,0")}</span> 元
                        </p>
                        <p style='margin: 0.5em 0;'>
                            <span>總價新台幣： </span>
                            <span style='width: 2em; display: inline-block; text-align-last: justify;'>NT$</span>：
                            <span style='width: 7em; display: inline-block; border-bottom: 1px solid; text-align: right;'>{afterTaxTotal.ToString("#,0")}</span> 元
                        </p>
                        ";
                }
                else
                {
                    totalTemplate = $@"
                        <p style='margin: 0.5em 0;'>
                            <span>總價新台幣： </span>
                            <span style='width: 2em; display: inline-block; text-align-last: justify;'>NT$</span>：
                            <span style='width: 7em; display: inline-block; border-bottom: 1px solid; text-align: right;'>{totalAmount.ToString("#,0")}</span> 元
                        </p>
                        ";
                }

                string template = $@"
<body style='box-sizing: border-box; font-family:微軟正黑體; margin: 0; font-size: 12px; padding: 0 45px;'>
    <section style='box-sizing: border-box; width: 100%; padding: 20px 20px;'>
        <div style='box-sizing: border-box; text-align: center; font-size: 30px;'>
            <p style='margin: 0.5em 0;'>政達冷氣有限公司</p>
        </div>
        <div style='box-sizing: border-box;'>
            <div style='box-sizing: border-box; display: inline-block; line-height: 1.5;'>
                <p style='margin: 0;'>冷氣各機種</p>
                <p style='margin: 0;'>移機/裝機/保養/維修</p>
            </div>
            <div style='box-sizing: border-box; display: inline-block; float: right; line-height: 1.5;'>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>案&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;號</span>：
                    <span></span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>頁&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;次</span>：
                    <span>1/1</span>
                </p>
            </div>
        </div>
        <div style='box-sizing: border-box; text-align: center; font-size: 25px;'>
            <p style='margin: 0.5em 0;'>估&nbsp;&nbsp;價&nbsp;&nbsp;單</p>
        </div>
        <div style='box-sizing: border-box;'>
            <div style='box-sizing: border-box; display: inline-block; line-height: 1.5;'>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>業主名稱</span>：
                    <span>{req.customer_name}</span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>工程名稱</span>：
                    <span>{req.project_name}</span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>工程地址</span>：
                    <span>{req.customer_address}</span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>統一編號</span>：
                    <span>{req.companyTax}</span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>傳&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;真</span>：
                    <span>{req.fax}</span>
                </p>
            </div>
            <div style='box-sizing: border-box; display: inline-block; float: right; line-height: 1.5;'>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>報價日期</span>：
                    <span>{DateTime.Now.ToString("yyyy/MM/dd")}</span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>連&nbsp;&nbsp;絡&nbsp;&nbsp;人</span>：
                    <span>{req.contact_name}</span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>電&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;話</span>：
                    <span>{req.phone}</span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>行動電話</span>：
                    <span>{req.mobile}</span>
                </p>
            </div>
        </div>
    </section>
    <section style='box-sizing: border-box; width: 100%; padding: 0px 20px 20px;'>
        <table style='box-sizing: border-box; width: 100%; border-collapse: collapse; font-size: 12px;'>
            <thead>
                <tr>
                    <th style='padding: 0 0.5em; border: 1px solid; width: 3em;'>項目</th>
                    <th style='padding: 0 0.5em; border: 1px solid;'>品名/規格</th>
                    <th style='padding: 0 0.5em; border: 1px solid;'>單位</th>
                    <th style='padding: 0 0.5em; border: 1px solid;'>數量</th>
                    <th style='padding: 0 0.5em; border: 1px solid;'>單價</th>
                    <th style='padding: 0 0.5em; border: 1px solid;'>複價</th>
                    <th style='padding: 0 0.5em; border: 1px solid; width: 20%;'>備註</th>
                </tr>
            </thead>
            <tbody>
                {rowAssemble}
            </tbody>
            <tfoot>
                <tr>
                    <td colspan='7' style='padding: 0.5em 15%; border: 1px solid; text-align: right;'>
                        {totalTemplate}
                    </td>
                </tr>
                <tr>
                    <td colspan='7' style='padding: 0.5em; border: 1px solid;'>
                        <span style='font-size: 0.8em;'>備註：</span>
                        <div style='box-sizing: border-box;'>
                            <p style='word-break: break-all; margin: 0;'></p>
                        </div>
                    </td>
                </tr>
            </tfoot>
        </table>
    </section>
    <section style='box-sizing: border-box; width: 100%; padding: 0px 20px 80px;'>
        <div style='box-sizing: border-box;'>
            <div style='box-sizing: border-box; display: inline-block; line-height: 1.5;'>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>付款方式</span>：
                    <span></span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>發票號碼</span>：
                    <span></span>
                </p>
                <p style='margin: 0;'>
                    <span style='display: inline-block; text-align-last: justify;'>PS</span>：
                    <span>1.本報價單有效期為七天。</span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 2.15em; display: inline-block; text-align-last: justify;'></span>
                    <span>2.如支票不能如期兌現貨物所有權仍歸屬賣方所有。</span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>公司電話</span>：
                    <span>(02)2689-9351 / (02)2676-3799</span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>公司傳真</span>：
                    <span>(02)2676-4718</span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>行動電話</span>：
                    <span>{cellPhone}</span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>公司地址</span>：
                    <span>新北市樹林區光興街47號</span>
                </p>
            </div>
            <div style='box-sizing: border-box; display: inline-block; float: right; line-height: 1.5;'>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>完工日期</span>：
                    <span></span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>經辦人</span>：
                    <span>{req.create_user} 先生</span>
                </p>
                <br><br>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>業主認簽</span>：
                    <span style='width: 12em; display: inline-block; border: 1px solid;'></span>
                </p>
            </div>
        </div>
    </section>
</body>
";
                var doc = new HtmlToPdfDocument()
                {
                    GlobalSettings =
                    {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings { Top = 0, Left = 0, Right = 0, Bottom= 0 }
                    },
                    Objects =
                    {
                        new ObjectSettings
                            {
                                PagesCount = true,
                                HtmlContent = template,
                                WebSettings = new WebSettings
                                {
                                    PrintMediaType = true,
                                    EnableIntelligentShrinking = false,
                                    DefaultEncoding = "utf-8"
                                },
                                HeaderSettings = null,
                                FooterSettings = null
                            }
                    }
                };

                var buffer = _pdfConverter.Convert(doc);
                return new MemoryStream(buffer);
            }
            catch (Exception ex)
            {


            }

            return null;
        }

        [HttpPost]
        [Route("Export")]
        public async Task<Stream> ExportAsync(ExportReq req)
        {
            try
            {
                var now = DateTime.Now;
                Quote quote = new Quote();
                using (var trans = _database.BeginTransaction())
                {
                    try
                    {
                        quote = await _quoteRepository.Insert(new Quote
                        {
                            case_number = now.ToString("yyyyMMddHHmmss"),
                            project_name = req.project_name,
                            customer_name = req.customer_name,
                            customer_address = req.customer_address,
                            contact_name = req.contact_name,
                            phone = req.phone,
                            mobile = req.mobile,
                            companyTax = req.companyTax,
                            fax = req.fax,
                            create_time = now,
                            create_user = req.create_user,
                            fax_type = req.fax_type
                        });
                        await _database.SaveChangedAsync();

                        foreach (var item in req.items)
                        {
                            await _quoteDetailRepository.Insert(new QuoteDetail
                            {
                                qid = quote.id,
                                product = item.product,
                                quantity = item.quantity,
                                pattern = item.pattern,
                                unit_price = item.unit_price,
                                amount = item.amount,
                                remark = item.remark,
                                idx = item.idx
                            });
                        }
                        await _database.SaveChangedAsync();
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw new Exception(GenericResType.DB_ERROR, ex);
                    }
                }
                string cellPhone = req.create_user == "劉居政" ? "0932-227-943" : "0932-121-776";
                #region 項目
                int totalAmount = 0; //總價
                int taxAmount = 0;  //稅額
                int rows = 20;
                string rowAssemble = string.Empty;
        

                for (int i = 1; i <= rows; i++)
                {
                    if (i <= req.items.Count)
                    {
                        var tmp = req.items[i - 1];
                        rowAssemble += $@"
                        <tr>
                            <td style='padding: 0 0.5em; border: 1px solid; height: 1.5em; text-align: center;'>{i}</td>
                            <td style='padding: 0 0.5em; border: 1px solid; height: 1.5em;'>{tmp.product}</td>
                            <td style='padding: 0 0.5em; border: 1px solid; height: 1.5em; text-align: center;'>{tmp.pattern}</td>
                            <td style='padding: 0 0.5em; border: 1px solid; height: 1.5em; text-align: center;'>{(tmp.quantity == 0 ? "" : tmp.quantity.ToString("#,0"))}</td>
                            <td style='padding: 0 0.5em; border: 1px solid; height: 1.5em; text-align: right;'>{(tmp.unit_price == 0 ? "" : tmp.unit_price.ToString("#,0"))}</td>
                            <td style='padding: 0 0.5em; border: 1px solid; height: 1.5em; text-align: right;'>{(tmp.amount == 0 ? "" : tmp.amount.ToString("#,0"))}</td>
                            <td style='padding: 0 0.5em; border: 1px solid; height: 1.5em; word-break: break-all;'>{tmp.remark}</td>
                        </tr>";
                        totalAmount += tmp.amount;
                    }
                    else
                    {
                        rowAssemble += $@"
                        <tr>
                            <td style='padding: 0 0.5em; border: 1px solid; height: 1.5em; text-align: center;'></td>
                            <td style='padding: 0 0.5em; border: 1px solid; height: 1.5em;'></td>
                            <td style='padding: 0 0.5em; border: 1px solid; height: 1.5em; text-align: center;'></td>
                            <td style='padding: 0 0.5em; border: 1px solid; height: 1.5em; text-align: center;'></td>
                            <td style='padding: 0 0.5em; border: 1px solid; height: 1.5em; text-align: right;'></td>
                            <td style='padding: 0 0.5em; border: 1px solid; height: 1.5em; text-align: right;'></td>
                            <td style='padding: 0 0.5em; border: 1px solid; height: 1.5em; word-break: break-all;'></td>
                        </tr>";
                    }

                }

                taxAmount = (int)Math.Round(totalAmount * 0.05, 0, MidpointRounding.AwayFromZero);
                #endregion

                int afterTaxTotal = 0;
                string totalTemplate = "";
                // 1: 外加  2: 內含
                if (req.fax_type == "1" || req.fax_type == "2")
                {
                    afterTaxTotal = req.fax_type == "1" ? (totalAmount + taxAmount) : totalAmount;
                    totalTemplate = $@"
                        <p style='margin: 0.5em 0;'>
                            <span style='width: 2em; display: inline-block; text-align-last: justify;'>NT$</span>：
                            <span style='width: 7em; display: inline-block; text-align: right;'>{totalAmount.ToString("#,0")}</span> 元
                        </p>
                        <p style='margin: 0.5em 0;'>
                            <span style='width: 2em; display: inline-block; text-align-last: justify;'>稅額</span>：
                            <span style='width: 7em; display: inline-block; border-bottom: 1px solid; text-align: right;'>{taxAmount.ToString("#,0")}</span> 元
                        </p>
                        <p style='margin: 0.5em 0;'>
                            <span>總價新台幣： </span>
                            <span style='width: 2em; display: inline-block; text-align-last: justify;'>NT$</span>：
                            <span style='width: 7em; display: inline-block; border-bottom: 1px solid; text-align: right;'>{afterTaxTotal.ToString("#,0")}</span> 元
                        </p>
                        ";
                }
                else
                {
                    totalTemplate = $@"
                        <p style='margin: 0.5em 0;'>
                            <span>總價新台幣： </span>
                            <span style='width: 2em; display: inline-block; text-align-last: justify;'>NT$</span>：
                            <span style='width: 7em; display: inline-block; border-bottom: 1px solid; text-align: right;'>{totalAmount.ToString("#,0")}</span> 元
                        </p>
                        ";
                }

                string template = $@"
<body style='box-sizing: border-box; font-family:微軟正黑體; margin: 0; font-size: 12px; padding: 0 45px;'>
    <section style='box-sizing: border-box; width: 100%; padding: 20px 20px;'>
        <div style='box-sizing: border-box; text-align: center; font-size: 30px;'>
            <p style='margin: 0.5em 0;'>政達冷氣有限公司</p>
        </div>
        <div style='box-sizing: border-box;'>
            <div style='box-sizing: border-box; display: inline-block; line-height: 1.5;'>
                <p style='margin: 0;'>冷氣各機種</p>
                <p style='margin: 0;'>移機/裝機/保養/維修</p>
            </div>
            <div style='box-sizing: border-box; display: inline-block; float: right; line-height: 1.5;'>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>案&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;號</span>：
                    <span>{quote.case_number}</span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>頁&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;次</span>：
                    <span>1/1</span>
                </p>
            </div>
        </div>
        <div style='box-sizing: border-box; text-align: center; font-size: 25px;'>
            <p style='margin: 0.5em 0;'>估&nbsp;&nbsp;價&nbsp;&nbsp;單</p>
        </div>
        <div style='box-sizing: border-box;'>
            <div style='box-sizing: border-box; display: inline-block; line-height: 1.5;'>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>業主名稱</span>：
                    <span>{req.customer_name}</span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>工程名稱</span>：
                    <span>{req.project_name}</span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>工程地址</span>：
                    <span>{req.customer_address}</span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>統一編號</span>：
                    <span>{req.companyTax}</span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>傳&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;真</span>：
                    <span>{req.fax}</span>
                </p>
            </div>
            <div style='box-sizing: border-box; display: inline-block; float: right; line-height: 1.5;'>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>報價日期</span>：
                    <span>{DateTime.Now.ToString("yyyy/MM/dd")}</span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>連&nbsp;&nbsp;絡&nbsp;&nbsp;人</span>：
                    <span>{req.contact_name}</span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>電&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;話</span>：
                    <span>{req.phone}</span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>行動電話</span>：
                    <span>{req.mobile}</span>
                </p>
            </div>
        </div>
    </section>
    <section style='box-sizing: border-box; width: 100%; padding: 0px 20px 20px;'>
        <table style='box-sizing: border-box; width: 100%; border-collapse: collapse; font-size: 12px;'>
            <thead>
                <tr>
                    <th style='padding: 0 0.5em; border: 1px solid; width: 3em;'>項目</th>
                    <th style='padding: 0 0.5em; border: 1px solid;'>品名/規格</th>
                    <th style='padding: 0 0.5em; border: 1px solid;'>單位</th>
                    <th style='padding: 0 0.5em; border: 1px solid;'>數量</th>
                    <th style='padding: 0 0.5em; border: 1px solid;'>單價</th>
                    <th style='padding: 0 0.5em; border: 1px solid;'>複價</th>
                    <th style='padding: 0 0.5em; border: 1px solid; width: 20%;'>備註</th>
                </tr>
            </thead>
            <tbody>
                {rowAssemble}
            </tbody>
            <tfoot>
                <tr>
                    <td colspan='7' style='padding: 0.5em 15%; border: 1px solid; text-align: right;'>
                       {totalTemplate}
                    </td>
                </tr>
                <tr>
                    <td colspan='7' style='padding: 0.5em; border: 1px solid;'>
                        <span style='font-size: 0.8em;'>備註：</span>
                        <div style='box-sizing: border-box;'>
                            <p style='word-break: break-all; margin: 0;'></p>
                        </div>
                    </td>
                </tr>
            </tfoot>
        </table>
    </section>
    <section style='box-sizing: border-box; width: 100%; padding: 0px 20px 80px;'>
        <div style='box-sizing: border-box;'>
            <div style='box-sizing: border-box; display: inline-block; line-height: 1.5;'>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>付款方式</span>：
                    <span></span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>發票號碼</span>：
                    <span></span>
                </p>
                <p style='margin: 0;'>
                    <span style='display: inline-block; text-align-last: justify;'>PS</span>：
                    <span>1.本報價單有效期為七天。</span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 2.15em; display: inline-block; text-align-last: justify;'></span>
                    <span>2.如支票不能如期兌現貨物所有權仍歸屬賣方所有。</span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>公司電話</span>：
                    <span>(02)2689-9351 / (02)2676-3799</span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>公司傳真</span>：
                    <span>(02)2676-4718</span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>行動電話</span>：
                    <span>{cellPhone}</span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>公司地址</span>：
                    <span>新北市樹林區光興街47號</span>
                </p>
            </div>
            <div style='box-sizing: border-box; display: inline-block; float: right; line-height: 1.5;'>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>完工日期</span>：
                    <span></span>
                </p>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>經辦人</span>：
                    <span>{req.create_user} 先生</span>
                </p>
                <br><br>
                <p style='margin: 0;'>
                    <span style='width: 4em; display: inline-block; text-align-last: justify;'>業主認簽</span>：
                    <span style='width: 12em; display: inline-block; border: 1px solid;'></span>
                </p>
            </div>
        </div>
    </section>
</body>
";
                var doc = new HtmlToPdfDocument()
                {
                    GlobalSettings =
                    {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings { Top = 0, Left = 0, Right = 0, Bottom= 0 }
                    },
                    Objects =
                    {
                        new ObjectSettings
                            {
                                PagesCount = true,
                                HtmlContent = template,
                                WebSettings = new WebSettings
                                {
                                    PrintMediaType = true,
                                    EnableIntelligentShrinking = false,
                                    DefaultEncoding = "utf-8"
                                },
                                HeaderSettings = null,
                                FooterSettings = null
                            }
                    }
                };

                var buffer = _pdfConverter.Convert(doc);
                return new MemoryStream(buffer);
            }
            catch (Exception ex)
            {


            }

            return null;
        }

        [HttpGet]
        [Route("GetParameter")]
        public async Task<GetParametersRes> GetParameter()
        {
            GetParametersRes res = new GetParametersRes()
            {
                message = GenericResType.QUERY_SUCCESS
            };
            try
            {
                var datas = await _systemParameterRepository.GetAll();
                if (datas != null)
                {
                    res.datas = datas.Select(x => x.name).ToList();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(GenericResType.QUERY_FAIL + " ex:" + ex.ToString());

                res.isError = true;
                res.message = GenericResType.QUERY_FAIL;
            }

            return res;
        }

        [HttpGet]
        [Route("TransformCSV")]
        public String TransformCSV([FromForm] TransformCSVReq req)
        {
            List<string> totalList = new List<string>();
            using (var reader = new StreamReader(req.file.OpenReadStream(), Encoding.Default, true))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    List<string> values = line.Split(',').ToList();
                    values = values.Where(x => !string.IsNullOrEmpty(x)).ToList();
                    foreach (var item in values)
                    {
                        totalList.Add($"INSERT INTO [dbo].[system_parameter] VALUES ('Quota','{item}')");
                    }
                }
            }
            
            return string.Join(System.Environment.NewLine,totalList);

        }
    }
}
