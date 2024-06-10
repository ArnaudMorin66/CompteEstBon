//-----------------------------------------------------------------------
// <copyright file="CebSetting.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace CebBlazor.Maui.Code;

public class CebSetting {
    public bool MongoDb { get; set; }

#pragma warning disable CS8632 // L'annotation pour les types référence Nullable doit être utilisée uniquement dans le code au sein d'un contexte d'annotations '#nullable'.
    public string? MongoDbConnectionString { get; set; }
#pragma warning restore CS8632 // L'annotation pour les types référence Nullable doit être utilisée uniquement dans le code au sein d'un contexte d'annotations '#nullable'.

    public bool AutoCalcul { get; set; }
}