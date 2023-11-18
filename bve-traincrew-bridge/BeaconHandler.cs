using TrainCrew;

internal class BeaconHandler
{
    private float PreviousNextStaDistance = 0;
    public void Reset()
    {
        PreviousNextStaDistance = 0;
    }
    public void HandleBeacon(TrainState state)
    {
        if (state == null) return;
        // TASCに必要なビーコン
        HandleTascBeacon(state);
        // ATOに必要なビーコン(制限速度etc...)
    }

    private void HandleGeneralBeacon(TrainState state)
    {
        // Todo: 勾配
        // Todo: 制限速度
    }

    private void HandleTascBeacon(TrainState state)
    {
        if (state.nextStopType != "停車")
        {
            return;
        }
        foreach (var distance in new float[]{ 500, 300, 10 })
        {
            if (state.nextStaDistance >= distance ||  PreviousNextStaDistance < distance)
            {
                continue;
            }
            var beacon = new AtsPlugin.ATS_BEACONDATA();
            // SignalとDistanceはここでは使わないので未設定でOK
            beacon.Type = 1030;
            beacon.Optional = (int)(state.nextStaDistance * 1000);
            AtsPlugin.SetBeaconData(beacon);
        }
        PreviousNextStaDistance = state.nextStaDistance;

    }
}