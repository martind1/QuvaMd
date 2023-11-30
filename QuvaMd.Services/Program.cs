using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quva.Database.Models;
using Quva.Logging.Models;
using Quva.Logging.Utils;
using Quva.Services.Interfaces.Loading;
using Quva.Services.Interfaces.Shared;
using Quva.Services.Interfaces.WeighingOperation;
using Quva.Services.Loading;
using Quva.Services.Mapping;
using Quva.Services.Services.Loading;
using Quva.Services.Services.Shared;
using Quva.Services.Services.WeighingOperation;
using Quva.Services.Trigger;
using QuvaMd.Services.Services.Shared;
using QuvaMd.Services.Tests;
using Serilog;
using Serilog.Debugging;
using System.Security.Cryptography.X509Certificates;

namespace QuvaMd.Services;

internal class Program
{
    //public static IHost? host;
    private static string? User;
    private static string? Pw;
    private static string? Datasource;
    private static string? Certname;
    private static int Port;
    private static string? sqlCompatibility;
    private static bool UseAuditTrigger;
    private static void Main(string[] args)
    {
        SelfLog.Enable(
            msg => Console.WriteLine("Error from Serilog: {0}", msg));
        IConfiguration configSerilog = new ConfigurationBuilder()
            //.AddJsonFile(@"c:\Keyfiles\quva-config.json", true, true)
            .AddJsonFile(@"Devices.json", true, true)
            .Build();
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configSerilog)
            .CreateLogger();
        Log.Error("\r\n");
        Log.Error("Initializing Serilog....");
        if (args.Length > 0)
        {
            Log.Debug("args: " + string.Join(',', args));
        }
        //var cs = Crypt.EncryptString("b14ca5898a4e4133bbce2ea2315a1916", "QUVATESTNEU");
        //Log.Error($"cs:{cs}");

        User = configSerilog.GetSection("database").GetSection("user").Value;
        Pw = configSerilog.GetSection("database").GetSection("pw").Value;
        Datasource = configSerilog.GetSection("database").GetSection("datasource").Value;
        var conn = new SqlConnectionStringBuilder
        {
            Password = Pw,
            DataSource = Datasource,
            UserID = User,
            PersistSecurityInfo = false
        };
        sqlCompatibility = configSerilog.GetSection("database").GetSection("sqlCompatibility").Value ?? "11";
        string trigger = configSerilog.GetSection("application").GetSection("useAuditTrigger").Value ?? "false";
        UseAuditTrigger = bool.Parse(trigger);

        //Service dependency injection:
        var builder = WebApplication.CreateBuilder();

        // TK Controller:
        Certname = configSerilog.GetSection("application").GetSection("certname").Value;
        Port = Convert.ToInt32(configSerilog.GetSection("application").GetSection("port").Value);

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        //ToDo wieder einkommentieren
        builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
            .AddNegotiate();

        builder.Services.AddAuthorization(options =>
        {
            // By default, all incoming requests will be authorized according to the default policy.
            options.FallbackPolicy = options.DefaultPolicy;
        });

        //builder.Host.UseWindowsService();
        Log.Information("ConfigureKestrel");
        builder.WebHost.ConfigureKestrel(config =>
        {

            config.ConfigureEndpointDefaults(opt => opt.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1);

            config.ConfigureHttpsDefaults(opt =>
            {

                opt.OnAuthenticate = (conContext, sslAuthOptions) =>
                {
                    sslAuthOptions.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls11 |
                                                         System.Security.Authentication.SslProtocols.Tls12 |
                                                         System.Security.Authentication.SslProtocols.Tls13;
                    sslAuthOptions.CipherSuitesPolicy = null;
                    sslAuthOptions.AllowRenegotiation = false;
                };


                var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly);

                X509Certificate2Collection? cert = null;
                if (Certname != null)
                {
                    //cert = store.Certificates.Find(X509FindType.FindBySubjectName, Certname, false);
                    cert = store.Certificates.Find(X509FindType.FindByThumbprint, "2baaffcf9e778f0aa8436f72f5647ba268473e4b", false);
                }

                if (cert != null && cert.Count > 0)
                {
                    Log.Information("Zertifikat " + cert[0].FriendlyName + " gefunden");
                    opt.ServerCertificate = cert[0];
                }
                else
                {
                    cert = store.Certificates.Find(X509FindType.FindBySubjectName, "localhost", false);
                    if (cert != null && cert.Count > 0)
                    {
                        Log.Information("Zertifikat " + cert[0].FriendlyName + " gefunden");
                        opt.ServerCertificate = cert[0];
                    }
                    else
                    {
                        Log.Information("Kein Zertifikat gefunden");
                    }
                }
            });
        }).UseUrls("https://*:" + Port);

        builder.Services.AddControllers(options =>
        {
            options.MaxIAsyncEnumerableBufferLimit = int.MaxValue;

        });

        //UnityConfig.RegisterServices(builder.Services);

        string url = "https://" + Certname + ":5001";
        string urlQuva = "https://" + Certname + ":5000";

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("QuvaMdCors",
                                  policy =>
                                  {
                                      policy.WithOrigins(url, urlQuva, "https://localhost:5001", "https://localhost:5000").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                                  });
        });

        // TK Database
        builder.Services.AddDbContext<QuvaContext>((provider, opt) =>
        {
            opt.EnableSensitiveDataLogging();  //Serilog
            opt.LogTo(Log.ForContext<QuvaContext>().Information, LogLevel.Information, null);
            opt.UseOracle(conn.ConnectionString, opt =>
            {
                opt.UseOracleSQLCompatibility(sqlCompatibility);
            });
            opt.UseTriggers(triggerOpts =>
            {
                triggerOpts.AddTrigger<DeliveryHeadTrigger>();
                triggerOpts.AddTrigger<DeliveryPositionTrigger>();
            });
            if (UseAuditTrigger)
            {
                opt.AddInterceptors(new AuditInterceptor(provider.GetRequiredService<IServiceProvider>()));
            }

            //opt.AddInterceptors(new TriggerInterceptor());
        }, ServiceLifetime.Scoped);

        builder.Services.AddDbContextPool<AuditContext>(opt =>
        {
            opt.EnableSensitiveDataLogging();
            opt.LogTo(Log.ForContext<AuditContext>().Information, LogLevel.Information, null);
            opt.UseOracle(conn.ConnectionString, opt =>
            {
                opt.UseOracleSQLCompatibility(sqlCompatibility);
            });
        });

        builder.Host.UseSerilog();  //log sql
        //builder.Services.AddSerilogServices(builder.Configuration);
        builder.Services.AddSingleton(Log.Logger);
        builder.Services.AddMapster();


        //My Services:
        builder.Services.AddSingleton<IDeviceService, DeviceService>();
        builder.Services.AddScoped<ILocationParameterLoadingService, LocationParameterLoadingService>();
        builder.Services.AddScoped<IAgreementsService, AgreementsService>();
        builder.Services.AddScoped<ILoadingDbService, LoadingDbService>();
        builder.Services.AddScoped<IBasetypeService, BasetypeService>();
        builder.Services.AddScoped<ILoadOrderService, LoadOrderService>();
        builder.Services.AddScoped<ILoadingService, LoadingService>();
        builder.Services.AddScoped<ILoadInfoService, LoadInfoService>();
        builder.Services.AddScoped<IOdcAdjustmentQuantityService, OdcAdjustmentQuantityService>();
        builder.Services.AddScoped<ISapTransferService, SapTransferService>();
        builder.Services.AddScoped<IDeliveryBasetypeService, DeliveryBasetypeService>();
        //29.11.23:
        builder.Services.AddScoped<IDeliveryHeadService, DeliveryHeadService>();
        builder.Services.AddScoped<IOdcAdjustmentDayService, OdcAdjustmentDayService>();

        Log.Information("Services added");

        var app = builder.Build();

        // TK
        // Configure the HTTP request pipeline.
        //if (app.Environment.IsDevelopment())
        {
            Log.Information("UseSwagger");
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseRouting();
        app.UseCors("QuvaMdCors");

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        // Testmenü:
        while (true)
        {
            Console.WriteLine("### Startmenü ###");
            Console.WriteLine("0 = app.Run()");
            Console.WriteLine("1 = Test DeviceService");
            Console.WriteLine("2 = Test Locationparameter");
            Console.WriteLine("3 = Test CustomerAgreements");
            Console.WriteLine("4 = Test LoadingService");
            Console.WriteLine("5 = Test OdcAdjustmentQuantityService");
            Console.WriteLine("sonst = Ende");
            ConsoleKeyInfo key = Console.ReadKey(); //warten auf Taste
            Console.WriteLine("");
            if (key.KeyChar == '0')
            {
                app.Run();
                break;
            }
            else if (key.KeyChar == '1')
            {
                var svc = app.Services.GetRequiredService<IDeviceService>();
                var testsvc = new TestDeviceService(svc);
                testsvc.Menu();
            }
            else if (key.KeyChar == '2')
            {
                var svc = app.Services.GetRequiredService<ILocationParameterLoadingService>();
                var testobj = new TestLocationParameter(svc);
                testobj.Menu().GetAwaiter().GetResult();
            }
            else if (key.KeyChar == '3')
            {
                var svc = app.Services.GetRequiredService<IAgreementsService>();
                var testobj = new TestCustomerAgreement(svc);
                testobj.Menu().GetAwaiter().GetResult();
            }
            else if (key.KeyChar == '4')
            {
                var svc1 = app.Services.GetRequiredService<ILoadingService>();
                var svc2 = app.Services.GetRequiredService<IDeliveryBasetypeService>();
                var testobj = new TestLoadingService(svc1, svc2);
                testobj.Menu().GetAwaiter().GetResult();
            }
            else if (key.KeyChar == '5')
            {
                var svc1 = app.Services.GetRequiredService<IOdcAdjustmentQuantityService>();
                var svc2 = app.Services.GetRequiredService<IDeliveryBasetypeService>();
                var testobj = new TestOdcAdjustment(svc1, svc2);
                testobj.Menu().GetAwaiter().GetResult();
            }
            else
            {
                break;
            }
        }

    }
}

