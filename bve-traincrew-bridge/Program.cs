using System.Runtime.InteropServices;
using bve_traincrew_bridge;
using TrainCrew;

internal class Program
{
    private TimeSpan PreviousTime {  get; set; }
    private string PreviousDiaName { get; set; } = "";
    private bool PreviousDoorClose { get; set; } = true;
    private int PreviousPnotch = 0;
    private int PreviousBnotch = 0;
    private BeaconHandler Handler;
    
    public TascObject TascData { get; set; } = new();

    Program()
    {
        Handler = new BeaconHandler();
    }

    private static void Main(string[] args)
    {
        var program = new Program();
        var task = program.main(args);
        task.Wait();
    }

    private async Task main(string[] args)
    {
        TrainCrewInput.Init();
        
        // Load settings
        var config = new LoadConfig();
        
        // Start REST API
        if (config.RestApiEnable)
        {
            new RESTApi(config.RestApiPort, TascData);
        }
        
        try
        {
            loadPlugin();
            var firstLoop = true;
            while (true)
            {
                TrainCrewInput.RequestStaData();
                var timer = Task.Delay(16);
                var state = TrainCrewInput.GetTrainState();
                // 列車番号が変わっている場合、列車が変わっているので電車再読み込み
                if (state.diaName != PreviousDiaName || firstLoop)
                {
                    loadTrain(state);
                }
                // 明らかに時刻が戻っている場合は、最初からを選んだので路線のみ再読み込み
                // Todo: TrainCrew側でState実装されたらそちらに変更
                else if (state.NowTime < PreviousTime)
                {
                    loadDiagram();
                }
                
                // 時刻が進んでいれば、Elapseを呼ぶ Todo: State取得できるようになったらそれ使う Pause禁止
                if (state.NowTime > PreviousTime || true)
                {
                    // ドアの開閉処理
                    if (state.AllClose != PreviousDoorClose || firstLoop)
                    {
                        if (state.AllClose)
                        {
                            doorClose();
                        }
                        else
                        {
                            doorOpen();
                        }
                    }
                    // 手動操作をBVEプラグイン側に伝える
                    if(state.Pnotch != PreviousPnotch)
                    {
                        setPower(state.Pnotch);
                        PreviousPnotch = state.Pnotch;
                    }
                    if(state.Bnotch != PreviousBnotch)
                    {
                        setBrake(state.Bnotch);
                        PreviousBnotch = state.Bnotch;
                    }
                    // フレーム処理
                    var handle = elapse(state);
                    // 結果をTrainCrew側に反映
                    if (state.Pnotch != handle.Power)
                    {
                        TrainCrewInput.SetNotch(handle.Power);   
                    }
                    else if (state.Bnotch != handle.Brake)
                    {
                        TrainCrewInput.SetNotch(-handle.Brake);
                    }
                    PreviousPnotch = handle.Power;
                    PreviousBnotch = handle.Brake;

                    // TrainCrewInput.SetReverser(handle.Reverser);
                    // ビーコン処理
                    handleBeacon(state);
                }
                PreviousDoorClose = state.AllClose;
                PreviousDiaName = state.diaName;
                PreviousTime = state.NowTime;
                firstLoop = false;
                await timer;
            }
        }
        finally
        {
            TrainCrewInput.Dispose();
            disposePlugin();
        }
        
    }

    private void handleBeacon(TrainState state)
    {
        // 運転状況に合わせてビーコンを通過した旨をBVEプラグインに送る
        // ここは、プラグインや状況に応じてカスタムが必要になる
        Handler.HandleBeacon(state);
    }


    private void loadPlugin()
    {
        AtsPlugin.Load();
    }

    private void disposePlugin()
    {
        AtsPlugin.Dispose();
    }

    private void loadTrain(TrainState trainState)
    {
        var spec = new AtsPlugin.ATS_VEHICLESPEC();
        spec.Cars = trainState.CarStates.Count;
        spec.BrakeNotches = 8;
        spec.PowerNotches = 5;
        spec.B67Notch = 6;
        spec.AtsNotch = 8;
        AtsPlugin.SetVehicleSpec(spec);
        loadDiagram();
        setReverse(1);
    }

    private void loadDiagram()
    {
        Handler.Reset();
        AtsPlugin.Initialize(1);
    }

    private void PopulateTascData(int[] panelLamps)
    {
        // autopilot.iniパネル設定
        // 50 = tascenabled
        // 51 = tascmonitor
        // 52 = tascbrake
        // 53 = tascposition
        // 54 = inching
        
        // Populate TASC data into TascData
        TascData.Power = panelLamps[50] != 0;
        TascData.Monitor = panelLamps[51] != 0;
        TascData.Brake = panelLamps[52];
        TascData.Position = panelLamps[53];
        TascData.Inching = panelLamps[54] != 0;
    } 
    
    private AtsPlugin.ATS_HANDLES elapse(TrainState trainState)
    {
        var vehicleState = new AtsPlugin.ATS_VEHICLESTATE();
        // 現在位置の計算
        // 現在位置は次の駅との距離差分で計算する
        if (trainState.stationList.Count > trainState.nowStaIndex)
        {
            var nextStation = trainState.stationList[trainState.nowStaIndex];
            vehicleState.Location = nextStation.TotalLength - trainState.nextStaDistance;
        }
        else
        {
            vehicleState.Location = 0;
        }
        vehicleState.Speed = trainState.Speed;
        vehicleState.Time = (int)trainState.NowTime.TotalMilliseconds;
        // Todo: carState使ってブレーキシリンダ圧力を設定
        vehicleState.BcPressure = 0;
        vehicleState.MrPressure = trainState.MR_Press;
        // Todo: TrainCrew側で実装されたら正しい値に変える
        vehicleState.ErPressure = 0;
        vehicleState.BpPressure = 0;
        vehicleState.SapPressure = 0;
        // Todo: carState使って電流を設定
        vehicleState.Current = 0;
        var panel = new int[256];
        var sound = new int[256];
        var result = AtsPlugin.Elapse(vehicleState, panel, sound);
        
        PopulateTascData(panel);
        
        return result;
    }

    private void doorOpen()
    {
        AtsPlugin.DoorOpen();
    }

    private void doorClose()
    {
        AtsPlugin.DoorClose();
    }

    private void setPower(int notch)
    {
        AtsPlugin.SetPower(notch);
    }

    private void setBrake(int notch)
    {
        AtsPlugin.SetBrake(notch);
    }

    private void setReverse(int reverser)
    {
        AtsPlugin.SetReverser(reverser);

    }

}