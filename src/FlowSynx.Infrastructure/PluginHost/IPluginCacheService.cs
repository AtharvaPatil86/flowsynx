﻿using FlowSynx.PluginCore;

namespace FlowSynx.Infrastructure.PluginHost;

public interface IPluginCacheService
{
    PluginLoader? Get(string key);
    void Set(string key, PluginCacheIndex index, IPlugin value, 
        TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null);
    bool TryGetValue(string key, out PluginLoader? value);
    void RemoveByKey(string key);
    void RemoveByIndex(PluginCacheIndex index);
}