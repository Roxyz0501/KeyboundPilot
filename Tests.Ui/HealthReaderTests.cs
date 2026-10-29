using FFXIVClientStructs.FFXIV.Component.GUI;
using KeyboundPilot;
unsafe partial class Program
{
 static void CheckHealthReader()
 {
  void Check(bool value,string message){if(!value)throw new Exception(message);}
  AtkResNode root=default;root.NodeFlags=NodeFlags.Visible;root.Color.A=255;
  AtkComponentNode owner=default;owner.NodeFlags=NodeFlags.Visible;owner.Color.A=255;owner.ParentNode=&root;
  AtkComponentGaugeBar gauge=default;gauge.OwnerNode=&owner;gauge.MinValue=0;gauge.MaxValue=100;
  gauge.Values[0].ValueInt=49;gauge.Values[0].ValueFloatDecreasing=90;
  Check(HealthReader.ReadGauge(&gauge)==49,"use actual HP value instead of animated trail");
  gauge.Values[0].ValueInt=50;Check(HealthReader.ReadGauge(&gauge)==50,"recovery reads fresh value");
  gauge.MaxValue=1000;gauge.Values[0].ValueInt=499;
  Check(Math.Abs(HealthReader.ReadGauge(&gauge)!.Value-49.9f)<.001,"convert gauge range without integer rounding");
  gauge.Values[0].ValueInt=0;Check(HealthReader.ReadGauge(&gauge)==0,"zero health remains a valid measurement");
  gauge.Values[0].ValueInt=1000;Check(HealthReader.ReadGauge(&gauge)==100,"full HP");
  gauge.Values[0].ValueInt=1001;Check(HealthReader.ReadGauge(&gauge)==null,"reject value above maximum");
  gauge.Values[0].ValueInt=-1;Check(HealthReader.ReadGauge(&gauge)==null,"reject negative HP");
  gauge.Values[0].ValueInt=50;gauge.MaxValue=0;Check(HealthReader.ReadGauge(&gauge)==null,"uninitialized range is unavailable");
  gauge.MaxValue=100;gauge.MinValue=1;Check(HealthReader.ReadGauge(&gauge)==null,"unexpected lower bound unavailable");gauge.MinValue=0;
  owner.NodeFlags=0;Check(HealthReader.ReadGauge(&gauge)==null,"hidden gauge unavailable");owner.NodeFlags=NodeFlags.Visible;
  root.Color.A=0;Check(HealthReader.ReadGauge(&gauge)==null,"hidden parent unavailable");root.Color.A=255;
  root.ParentNode=&root;Check(HealthReader.ReadGauge(&gauge)==null,"parent traversal bounded");
  Check(HealthReader.ReadGauge(null)==null,"missing gauge unavailable");
  Console.WriteLine("PASS: native allied HP range, recovery, animated trail, invalid and hidden gauge guards");
 }
}
