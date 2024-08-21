﻿using System.Reflection;

namespace DumpLib.Helpers;

public class MethodHelper
{
    /// <summary>
    /// Method to get DeserializeObject from Newtonsoft assembly
    /// </summary>
    /// <returns>MethodInfo</returns>
    public static MethodInfo GetDeserializerMethodInfo()
    {
        try
        {
            return DataHelper._newtonAssembly.GetTypes().First(x =>
                x.Name == "JsonConvert"
            ).GetMethods().First(m =>
                m.Name == "DeserializeObject" &&
                m.IsGenericMethodDefinition &&
                m.GetParameters().Length == 1 &&
                m.GetParameters().Any(p =>
                    p.ParameterType == typeof(string)
                )
            );
        }
        catch (Exception e)
        {
            Utils.LogError("GetDeserializerMethodInfo");
            Utils.LogError(e);
            throw;
        }
    }

    /// <summary>
    /// Method to get Quit method from EFT (as of 20/05/2024 - GClass1955)
    /// </summary>
    /// <returns>MethodInfo</returns>
    public static MethodInfo GetApplicationQuitMethod()
    {
        try
        {
            return DataHelper._eftAssembly.GetTypes().First(x =>
                x.GetMethods().Any(y =>
                    y.Name == "Quit")
            ).GetMethod("Quit");
        }
        catch (Exception e)
        {
            Utils.LogError("GetApplicationQuitMethod");
            Utils.LogError(e);
            throw;
        }
    }

    public static MethodInfo GetToUnparsedDataMethod()
    {
        try
        {
            return DataHelper._eftAssembly.GetTypes().First(x =>
                x.GetMethods().Any(m =>
                    m.Name == "ToUnparsedData"
                )).GetMethod("ToUnparsedData");
        }
        catch (Exception e)
        {
            Utils.LogError("GetToUnparsedDataMethod");
            Utils.LogError(e);
            throw;
        }
    }
}