﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Harmony;
using MonomiPark.SlimeRancher;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Persist;
using SRML.SR.SaveSystem.Data.LandPlot;
using UnityEngine;
using VanillaLandPlotData = MonomiPark.SlimeRancher.Persist.LandPlotV08;
namespace SRML.SR.SaveSystem.Patches
{
    [HarmonyPatch(typeof(SavedGame))]
    internal static class PullLandPlotDataPatch
    {
        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(typeof(SavedGame), "Pull", new Type[] { typeof(GameModel), typeof(RanchV07) });
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
        {
            foreach (var v in instr)
            {
                if (v.opcode == OpCodes.Newobj && v.operand is ConstructorInfo con &&
                    con.DeclaringType == typeof(VanillaLandPlotData))
                {
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 3);
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(KeyValuePair<string, LandPlotModel>), "get_Value"));
                    yield return new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(PullLandPlotDataPatch),"CreateLandPlotData"));
                }
                else
                {
                    yield return v;
                }
            }
        }

        public static VanillaLandPlotData CreateLandPlotData(LandPlotModel model)
        {
            Debug.Log(model.typeId);
            var mod = SaveRegistry.ModForModelType(model.GetType());
            if (mod != null)
            {
                var info = SaveRegistry.GetSaveInfo(mod).GetRegistryFor<CustomLandPlotData>();
                var newmodel = info.GetDataForID(info.GetIDForModel(model.GetType()));
                newmodel.PullCustomModel(model);
                return newmodel;
            }
            return new VanillaLandPlotData();
        }
    }
}