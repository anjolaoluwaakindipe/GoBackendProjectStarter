namespace GoBackendProjectStarter.Utils;

public class ProjectText
{
    public static string MainGo(String moduleName)
    {
        return @$"
            package main

            import (
                ""{moduleName}/routes""
                ""{moduleName}/server""
                ""{moduleName}/services""
                ""{moduleName}/utils/config""
                ""{moduleName}/utils/logger""
                ""go.uber.org/fx""
            )

            func main() {{

                fx.New(
                    server.Module, 
                    config.Module, 
                    logger.Module, 
                    routes.RouteModule, 
                    routes.HandlerModule,
                    services.Module,
                ).Run()
            }}
        ";
    }

    public static string LoggerGo()
    {
        return @"
            package logger

            import (
                ""log""

                ""go.uber.org/fx""
                ""go.uber.org/zap""
                ""go.uber.org/zap/zapcore""
            )

            type Logger interface {
                Info(message string)
                Infoln(message string)
                Infof(template string , args ...interface{})
                Debug(message string)
                Error(message string)
                Fatal(message string)
                Fatalf(template string, args ...interface{})
                Fatalln(message string)
            }

            type ZapperLogger struct {
                logger *zap.Logger
            }

            func (zl *ZapperLogger) Info(message string) {
                zl.logger.Info(message)
            }

            func(zl *ZapperLogger) Infoln(message string){
                zl.logger.Sugar().Infoln(message)
            }

            func (zl *ZapperLogger) Infof(template string, args ...interface{}){
                zl.logger.Sugar().Infof(template , args...)
            }

            func (zl *ZapperLogger) Debug(message string) {
                zl.logger.Debug(message)
            }

            func (zl *ZapperLogger) Error(message string) {
                zl.logger.Error(message)
            }

            func (zl *ZapperLogger) Fatal(message string){
                zl.logger.Fatal(message)
            }

            func (z1 *ZapperLogger) Fatalf(template string, args ...interface{}){
                z1.logger.Sugar().Fatalf(template, args...)
            }

            func (zl *ZapperLogger) Fatalln(message string){
                zl.logger.Sugar().Fatalln(message)
            }

            func NewZapperLogger() *ZapperLogger {
                // zapper configurations
                zapConfig := zap.NewProductionConfig()

                zapConfig.EncoderConfig.TimeKey = ""timestamp""
                zapConfig.EncoderConfig.EncodeTime = zapcore.ISO8601TimeEncoder

                logger, err := zapConfig.Build(zap.AddCallerSkip(1))
                if err != nil {
                    log.Fatalln(""Error while initializing Zap Logger: "" + err.Error())
                }

                return &ZapperLogger{logger: logger}
            }

            var Module = fx.Module(""logger"", fx.Provide(fx.Annotate(NewZapperLogger, fx.As(new(Logger)))))

        ";
    }

    public static String AppConfigGo(String moduleName)
    {
     return $@"
        package config

        import (
            ""log""
            ""reflect""

            ""{moduleName}/utils/logger""
            ""github.com/spf13/viper""
            ""go.uber.org/fx""
        )

        type AppConfig struct {{
            Port string `mapstructure:""SERVER_PORT""`
            Env  string `mapstructure:""SERVER_ENV""`
        }}


        // Get environment variable and parse it into viper
        func parseEnvIntoViper(i interface{{}}, viperConfig *viper.Viper, parent string, delim string) error {{
            // get the type of AppConfig struct
            r := reflect.TypeOf(i)

            // Assign to a struct value if r is a pointer
            if r.Kind() == reflect.Pointer {{
                r = r.Elem()
            }}

            // loop through all the fields of the sturct 
            for i := 0; i < r.NumField(); i++ {{
                // get the mapstructure tag from each field
                field := r.Field(i)
                env := field.Tag.Get(""mapstructure"")

                // if there is a parent reassign env to be parent-delimiter-child e.g DATABASE_PORT. ""_"" is the delimiter
                if parent != """" {{
                    env = parent + delim + env
                }}

                // if the field contains a nested struct, recursively pass in that nested struct
                if field.Type.Kind() == reflect.Struct {{
                    t := reflect.New(field.Type).Elem().Interface()
                    parseEnvIntoViper(t, viperConfig, env, delim)
                    continue
                }}

                // if no nested struct, bind the value of the mapstructure to the viper config
                if err := viperConfig.BindEnv(env); err != nil {{
                    // return an error if there is one
                    return err
                }}
            }}

            // return nil if everything was successful
            return nil
        }}

        // checks to see if appconfig has values for all its fields
        func isAppConfigComplete(i interface{{}}, logger logger.Logger) {{
            // gets the value and type of app config
            r := reflect.TypeOf(i)
            v := reflect.ValueOf(i)

            // checks if appConfig is a pointer and assigns r and v to the value of the pointer
            if r.Kind() == reflect.Pointer {{
                r = r.Elem()
                v = v.Elem()
            }}

            // iterate through each field of appconfig	
            for i := 0; i < r.NumField(); i++ {{

                // if a field contains a nested struct call the function on that field
                if r.Field(i).Type.Kind() == reflect.Struct {{
                    isAppConfigComplete(v.Field(i).Interface(), logger)
                }}

                // crate a zero value of that field
                emptyValue := reflect.Zero(v.Field(i).Type()).Interface()

                // check to see if any field is empty i.e the field matches its respective zero value
                if v.Field(i).Interface() == emptyValue {{
                    logger.Fatalf(""Can't find Environment Variable: %s"", r.Field(i).Tag.Get(""mapstructure""))
                }}
            }}
        }}

        func NewAppConfigViper(logger logger.Logger) AppConfig {{
            // create an empty app config
            var ac AppConfig

            // set up the viper config instance
            viperConfig := viper.New()
            viperConfig.SetConfigFile("".env"")
            viperConfig.SetConfigType(""env"")
            viperConfig.AddConfigPath(""."")

            // if there is a .env file, load it into viperConfig
            if err := viperConfig.ReadInConfig(); err != nil {{

                // if an error occured, use parseEnvIntoViper() to load environemnt variables into viper instance
                logger.Infoln(""No configuration file found, using environment variables"")
                viperConfig.AutomaticEnv()
                if err := parseEnvIntoViper(&ac, viperConfig, """", ""_""); err != nil {{
                    log.Fatalln(""Error parsing environment variables"", err.Error())
                }}
            }}

            // marshal environment variables from viper config into appconfig instance
            if err := viperConfig.Unmarshal(&ac); err != nil {{
                logger.Fatalln(""Error while unmarshalling config/environment variables"" + err.Error())
            }}

            // check if all fields in appconfig have non-zero/non-default values
            isAppConfigComplete(&ac, logger)
            return ac
        }}

        var Module = fx.Module(""config"", fx.Provide(NewAppConfigViper))
     ";
    }

    public static String ServerGo(String moduleName)
    {
        return @$"
            package server

            import (
                ""context""
                ""net/http""

                ""{moduleName}/routes""
                ""{moduleName}/utils/config""
                ""{moduleName}/utils/logger""
                ""github.com/gorilla/mux""
                ""go.uber.org/fx""
            )

            // injected param struct from fx
            type InitServerParams struct {{
                fx.In
                AppConfig config.AppConfig
                Logger    logger.Logger
                Routes    []routes.Route `group:""routes""`
            }}

            func InitServer(lc fx.Lifecycle, isp InitServerParams) *http.Server {{
                // create mutiplexer from gorilla mux
                gorrillaMux := mux.NewRouter()

                // get all routes from injected params
                routes := isp.Routes

                isp.Logger.Infoln(""Registering routes..."")

                // Hanling all routes with there respective hanlers
                for i :=0 ; i < len(routes); i++ {{
                    subRoutes := routes[i].Routes()

                    for j := 0 ; j < len(subRoutes) ; j++ {{
                        gorrillaMux.HandleFunc(subRoutes[j].Path, subRoutes[j].HandlerFunc).Methods(subRoutes[j].Methods...)
                    }}
                }}

                // global middleware
                gorrillaMux.Use(mux.CORSMethodMiddleware(gorrillaMux))

                // create http server
                httpServer := &http.Server{{
                    Addr:    ""localhost:"" + isp.AppConfig.Port,
                    Handler: gorrillaMux,
                }}

                // fx lifecycle methods
                lc.Append(fx.Hook{{

                    OnStart: func(ctx context.Context) error {{
                        
                        // start server  
                        go func() {{

                            isp.Logger.Infof(""Server Starting on port %s... \n"", isp.AppConfig.Port)
                            if err := httpServer.ListenAndServe(); err != http.ErrServerClosed {{
                                isp.Logger.Fatal(""Could not start server: "" + err.Error())
                            }}

                        }}()
                        return nil
                    }},

                    OnStop: func(ctx context.Context) error {{
                        // gracefully shutdown server with context
                        if err := httpServer.Shutdown(ctx); err != nil {{
                            isp.Logger.Fatalln(""Error occured while gracefully shutdown server!!! "" + err.Error())
                            return err
                        }}
                        isp.Logger.Infoln(""Serving Gracefully terminated..."")
                        return nil
                    }},
                }})

                return httpServer
            }}

            var Module = fx.Module(""server"", fx.Provide(InitServer), fx.Invoke(func(*http.Server) {{}}))

        ";
    }

    public static String RouteGo(String moduleName)
    {
        return @$"
            package routes

            import (
                ""encoding/json""
                ""net/http""

                ""go.uber.org/fx""
            )

            type Route interface {{
                Routes() []RouteHandler
            }}

            // What every method of a handler should return
            // and also what every route's ""Routes()"" method should return
            type RouteHandler struct {{
                Path    string
                Methods []string
                http.HandlerFunc
            }}

            // Wraps any struct that has an interface of ""Route"" with fx Annotations
            // that allow it to be used by the server
            func RegisterRoute(r interface{{}}) interface{{}} {{
                return fx.Annotate(r, fx.As(new(Route)), fx.ResultTags(`group:""routes""`))
            }}

            var RouteModule = fx.Module(""routes"", fx.Provide(
                RegisterRoute(NewHelloRoute),
            ))

            var HandlerModule = fx.Module(""handlers"", fx.Provide(
                NewHelloHandler,
            ))

            func res(rw http.ResponseWriter, body interface{{}}, code int) {{
                rw.Header().Add(""Content-Type"", ""application/json"")
                rw.WriteHeader(code)
                json.NewEncoder(rw).Encode(body)
            }}

            ";
    }

    public static String HelloRouteGo(string moduleName)
    {
        return @$"
            package routes

            import ""{moduleName}/utils/logger""

            type HelloRoute struct {{
                helloHandler HelloHandler
                logger logger.Logger
            }}

            func (hr HelloRoute) Routes() []RouteHandler {{
                hr.logger.Info(""Registering Hello Route"")
                routes := []RouteHandler{{
                    hr.helloHandler.getHello(),
                }}
                return routes
            }}

            func NewHelloRoute(helloHandler HelloHandler, logger logger.Logger) HelloRoute {{
                return HelloRoute{{helloHandler: helloHandler, logger: logger}};
            }}

        ";
    }

    public static String HelloHandler(string moduleName)
    {
        return @$"
            package routes

            import (
                ""encoding/json""
                ""net/http""
                ""{moduleName}/services""
            )

            type HelloHandler struct {{
                helloService services.HelloServiceInterface
            }}

            func (hh HelloHandler) getHello() RouteHandler {{
                return RouteHandler{{
                    Path:    ""/hello"",
                    Methods: []string{{""GET""}},
                    HandlerFunc: func(rw http.ResponseWriter, r *http.Request) {{
                        r.Header.Add(""Content-Type"", ""application/json"")
                        json.NewEncoder(rw).Encode(map[string]interface{{}}{{
                            ""hello"": hh.helloService.SayHello(),
                        }})
                    }},
                }}
            }}

            func NewHelloHandler (helloService services.HelloServiceInterface) HelloHandler {{
                return HelloHandler{{helloService: helloService}}
            }}

        ";
    }

    public static String EnvExample()
    {
        return @"
            SERVER_PORT=8080 
            SERVER_ENV=development
        ";
    }

    public static  String Env()
    {
        return @"
            SERVER_PORT=8080 
            SERVER_ENV=development
        ";
    }

    public static String ServiceGo()
    {
        return @"
            package services

            import ""go.uber.org/fx""

            var Module = fx.Module(""services"", fx.Provide(
                fx.Annotate(NewHelloService, fx.As(new(HelloServiceInterface))),
                ))

        ";
    }

    public static String HelloServiceInterface()
    {
        return @"
            package services;

            type HelloServiceInterface interface {
                SayHello() string
            }
        ";
    }

    public static String HelloServiceGo()
    {
        return $@"
            package services;

            type HelloService struct {{}}

            func (hs HelloService) SayHello () string {{
                return ""Hello""
            }}

            func NewHelloService() HelloService {{
                return HelloService{{}}
            }}
        ";
    }
}