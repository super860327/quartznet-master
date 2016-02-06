#region License
/* 
 * All content copyright Terracotta, Inc., unless otherwise indicated. All rights reserved. 
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not 
 * use this file except in compliance with the License. You may obtain a copy 
 * of the License at 
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0 
 *   
 * Unless required by applicable law or agreed to in writing, software 
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations 
 * under the License.
 * 
 */
#endregion

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Security;

using Common.Logging;

using Quartz.Core;
using Quartz.Impl.AdoJobStore;
using Quartz.Impl.AdoJobStore.Common;
using Quartz.Impl.Matchers;
using Quartz.Simpl;
using Quartz.Spi;
using Quartz.Util;

namespace Quartz.Impl
{
    public class StdSchedulerFactory : ISchedulerFactory
    {
        private const string ConfigurationKeyPrefix = "quartz.";
        private const string ConfigurationKeyPrefixServer = "quartz.server";
        public const string ConfigurationSectionName = "quartz";
        public const string PropertiesFile = "quartz.config";
        public const string PropertySchedulerInstanceName = "quartz.scheduler.instanceName";
        public const string PropertySchedulerInstanceId = "quartz.scheduler.instanceId";
        public const string PropertySchedulerInstanceIdGeneratorPrefix = "quartz.scheduler.instanceIdGenerator";
        public const string PropertySchedulerInstanceIdGeneratorType = PropertySchedulerInstanceIdGeneratorPrefix + ".type";
        public const string PropertySchedulerThreadName = "quartz.scheduler.threadName";
        public const string PropertySchedulerBatchTimeWindow = "quartz.scheduler.batchTriggerAcquisitionFireAheadTimeWindow";
        public const string PropertySchedulerMaxBatchSize = "quartz.scheduler.batchTriggerAcquisitionMaxCount";
        public const string PropertySchedulerExporterPrefix = "quartz.scheduler.exporter";
        public const string PropertySchedulerExporterType = PropertySchedulerExporterPrefix + ".type";
        public const string PropertySchedulerProxy = "quartz.scheduler.proxy";
        public const string PropertySchedulerProxyType = "quartz.scheduler.proxy.type";
        public const string PropertySchedulerIdleWaitTime = "quartz.scheduler.idleWaitTime";
        public const string PropertySchedulerDbFailureRetryInterval = "quartz.scheduler.dbFailureRetryInterval";
        public const string PropertySchedulerMakeSchedulerThreadDaemon = "quartz.scheduler.makeSchedulerThreadDaemon";
        public const string PropertySchedulerTypeLoadHelperType = "quartz.scheduler.typeLoadHelper.type";
        public const string PropertySchedulerJobFactoryType = "quartz.scheduler.jobFactory.type";
        public const string PropertySchedulerJobFactoryPrefix = "quartz.scheduler.jobFactory";
        public const string PropertySchedulerInterruptJobsOnShutdown = "quartz.scheduler.interruptJobsOnShutdown";
        public const string PropertySchedulerInterruptJobsOnShutdownWithWait = "quartz.scheduler.interruptJobsOnShutdownWithWait";
        public const string PropertySchedulerContextPrefix = "quartz.context.key";
        public const string PropertyThreadPoolPrefix = "quartz.threadPool";
        public const string PropertyThreadPoolType = "quartz.threadPool.type";
        public const string PropertyJobStorePrefix = "quartz.jobStore";
        public const string PropertyJobStoreLockHandlerPrefix = PropertyJobStorePrefix + ".lockHandler";
        public const string PropertyJobStoreLockHandlerType = PropertyJobStoreLockHandlerPrefix + ".type";
        public const string PropertyTablePrefix = "tablePrefix";
        public const string PropertySchedulerName = "schedName";
        public const string PropertyJobStoreType = "quartz.jobStore.type";
        public const string PropertyDataSourcePrefix = "quartz.dataSource";
        public const string PropertyDbProvider = "quartz.dbprovider";
        public const string PropertyDbProviderType = "connectionProvider.type";
        public const string PropertyDataSourceProvider = "provider";
        public const string PropertyDataSourceConnectionString = "connectionString";
        public const string PropertyDataSourceConnectionStringName = "connectionStringName";
        public const string PropertyPluginPrefix = "quartz.plugin";
        public const string PropertyPluginType = "type";
        public const string PropertyJobListenerPrefix = "quartz.jobListener";
        public const string PropertyTriggerListenerPrefix = "quartz.triggerListener";
        public const string PropertyListenerType = "type";
        public const string DefaultInstanceId = "NON_CLUSTERED";
        public const string PropertyCheckConfiguration = "quartz.checkConfiguration";
        public const string AutoGenerateInstanceId = "AUTO";
        public const string PropertyThreadExecutor = "quartz.threadExecutor";
        public const string PropertyThreadExecutorType = "quartz.threadExecutor.type";
        public const string PropertyObjectSerializer = "quartz.serializer";

        public const string SystemPropertyAsInstanceId = "SYS_PROP";

        private SchedulerException initException;

        private PropertiesParser cfg;

        private static readonly ILog log = LogManager.GetLogger(typeof(StdSchedulerFactory));

        private string SchedulerName
        {
            get { return cfg.GetStringProperty(PropertySchedulerInstanceName, "QuartzScheduler"); }
        }

        protected ILog Log
        {
            get { return log; }
        }

        public static IScheduler GetDefaultScheduler()
        {
            StdSchedulerFactory fact = new StdSchedulerFactory();
            return fact.GetScheduler();
        }

        public virtual ICollection<IScheduler> AllSchedulers
        {
            get { return SchedulerRepository.Instance.LookupAll(); }
        }

        public StdSchedulerFactory()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StdSchedulerFactory"/> class.
        /// </summary>
        /// <param name="props">The props.</param>
        public StdSchedulerFactory(NameValueCollection props)
        {
            Initialize(props);
        }

        public void Initialize()
        {
            if (cfg != null) return;

            if (initException != null) throw initException;

            NameValueCollection props = (NameValueCollection)ConfigurationManager.GetSection(ConfigurationSectionName);

            string requestedFile = QuartzEnvironment.GetEnvironmentVariable(PropertiesFile);

            string propFileName = requestedFile != null && requestedFile.Trim().Length > 0 ? requestedFile : "~/quartz.config";

            // check for specials
            try
            {
                propFileName = FileUtil.ResolveFile(propFileName);
            }
            catch (SecurityException)
            {
                log.WarnFormat("Unable to resolve file path '{0}' due to security exception, probably running under medium trust");
                propFileName = "quartz.config";
            }

            if (props == null && File.Exists(propFileName))
            {
                // file system
                try
                {
                    PropertiesParser pp = PropertiesParser.ReadFromFileResource(propFileName);
                    props = pp.UnderlyingProperties;
                    Log.Info(string.Format("Quartz.NET properties loaded from configuration file '{0}'", propFileName));
                }
                catch (Exception ex)
                {
                    Log.Error("Could not load properties for Quartz from file {0}: {1}".FormatInvariant(propFileName, ex.Message), ex);
                }

            }
            if (props == null)
            {
                // read from assembly
                try
                {
                    PropertiesParser pp = PropertiesParser.ReadFromEmbeddedAssemblyResource("Quartz.quartz.config");
                    props = pp.UnderlyingProperties;
                    Log.Info("Default Quartz.NET properties loaded from embedded resource file");
                }
                catch (Exception ex)
                {
                    Log.Error("Could not load default properties for Quartz from Quartz assembly: {0}".FormatInvariant(ex.Message), ex);
                }
            }
            if (props == null)
            {
                throw new SchedulerConfigException(
                    @"Could not find <quartz> configuration section from your application config or load default configuration from assembly.
Please add configuration to your application config file to correctly initialize Quartz.");
            }
            Initialize(OverrideWithSysProps(props));
        }

        private static NameValueCollection OverrideWithSysProps(NameValueCollection props)
        {
            NameValueCollection retValue = new NameValueCollection(props);
            IDictionary<string, string> vars = QuartzEnvironment.GetEnvironmentVariables();

            foreach (string key in vars.Keys)
            {
                retValue.Set(key, vars[key]);
            }

            return retValue;
        }

        public virtual void Initialize(NameValueCollection props)
        {
            cfg = new PropertiesParser(props);
            ValidateConfiguration();
        }

        protected virtual void ValidateConfiguration()
        {
            if (!cfg.GetBooleanProperty(PropertyCheckConfiguration, true)) return;

            List<string> supportedKeys = new List<string>();
            List<FieldInfo> fields = new List<FieldInfo>(GetType().GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy));
            fields = fields.FindAll(field => field.FieldType == typeof(string));

            foreach (FieldInfo field in fields)
            {
                string value = (string)field.GetValue(null);
                if (value != null && value.StartsWith(ConfigurationKeyPrefix) && value != ConfigurationKeyPrefix)
                {
                    supportedKeys.Add(value);
                }
            }

            foreach (string configurationKey in cfg.UnderlyingProperties.AllKeys)
            {
                if (!configurationKey.StartsWith(ConfigurationKeyPrefix) || configurationKey.StartsWith(ConfigurationKeyPrefixServer))
                {
                    continue;
                }

                bool isMatch = false;
                foreach (string supportedKey in supportedKeys)
                {
                    if (configurationKey.StartsWith(supportedKey, StringComparison.InvariantCulture))
                    {
                        isMatch = true;
                        break;
                    }
                }
                if (!isMatch)
                {
                    throw new SchedulerConfigException("Unknown configuration property '" + configurationKey + "'");
                }
            }
        }




        private IScheduler Instantiate()
        {
            if (cfg == null) Initialize();

            if (initException != null) throw initException;

            ISchedulerExporter exporter = null;
            IJobStore jobStore;
            IThreadPool threadPool;
            QuartzScheduler qs = null;
            IDbConnectionManager dbMgr = null;
            Type instanceIdGeneratorType = null;
            NameValueCollection tProps;
            bool autoId = false;
            TimeSpan idleWaitTime = TimeSpan.Zero;
            TimeSpan dbFailureRetry = TimeSpan.FromSeconds(15);
            IThreadExecutor threadExecutor;

            SchedulerRepository schedRep = SchedulerRepository.Instance;

            string schedName = cfg.GetStringProperty(PropertySchedulerInstanceName, "QuartzScheduler");
            string threadName = cfg.GetStringProperty(PropertySchedulerThreadName, "{0}_QuartzSchedulerThread".FormatInvariant(schedName));
            string schedInstId = cfg.GetStringProperty(PropertySchedulerInstanceId, DefaultInstanceId);


            //InstnceId 自动命名，随机数字还是系统环境变量
            if (schedInstId.Equals(AutoGenerateInstanceId))
            {
                autoId = true;
                instanceIdGeneratorType = LoadType(cfg.GetStringProperty(PropertySchedulerInstanceIdGeneratorType)) ?? typeof(SimpleInstanceIdGenerator);
            }
            else if (schedInstId.Equals(SystemPropertyAsInstanceId))
            {
                autoId = true;
                instanceIdGeneratorType = typeof(SystemPropertyInstanceIdGenerator);
            }

            idleWaitTime = cfg.GetTimeSpanProperty(PropertySchedulerIdleWaitTime, idleWaitTime);
            if (idleWaitTime > TimeSpan.Zero && idleWaitTime < TimeSpan.FromMilliseconds(1000))
            {
                throw new SchedulerException("quartz.scheduler.idleWaitTime of less than 1000ms is not legal.");
            }

            dbFailureRetry = cfg.GetTimeSpanProperty(PropertySchedulerDbFailureRetryInterval, dbFailureRetry);
            if (dbFailureRetry < TimeSpan.Zero)
            {
                throw new SchedulerException(PropertySchedulerDbFailureRetryInterval + " of less than 0 ms is not legal.");
            }

            bool makeSchedulerThreadDaemon = cfg.GetBooleanProperty(PropertySchedulerMakeSchedulerThreadDaemon);
            long batchTimeWindow = cfg.GetLongProperty(PropertySchedulerBatchTimeWindow, 0L);
            int maxBatchSize = cfg.GetIntProperty(PropertySchedulerMaxBatchSize, 1);

            bool interruptJobsOnShutdown = cfg.GetBooleanProperty(PropertySchedulerInterruptJobsOnShutdown, false);
            bool interruptJobsOnShutdownWithWait = cfg.GetBooleanProperty(PropertySchedulerInterruptJobsOnShutdownWithWait, false);

            NameValueCollection schedCtxtProps = cfg.GetPropertyGroup(PropertySchedulerContextPrefix, true);

            #region  Create type load helper
            ITypeLoadHelper loadHelper;
            try
            {
                Type typeLoadHelperType = LoadType(cfg.GetStringProperty(PropertySchedulerTypeLoadHelperType));
                loadHelper = ObjectUtils.InstantiateType<ITypeLoadHelper>(typeLoadHelperType ?? typeof(SimpleTypeLoadHelper));
            }
            catch (Exception e)
            {
                throw new SchedulerConfigException("Unable to instantiate type load helper: {0}".FormatInvariant(e.Message), e);
            }
            loadHelper.Initialize();
            #endregion

            #region proxyScheduler Create Remote Scheduler Proxy Factory
            bool proxyScheduler = cfg.GetBooleanProperty(PropertySchedulerProxy, false);

            // If Proxying to remote scheduler, short-circuit here...
            if (proxyScheduler)
            {
                if (autoId)
                {
                    schedInstId = DefaultInstanceId;
                }

                Type proxyType = loadHelper.LoadType(cfg.GetStringProperty(PropertySchedulerProxyType)) ?? typeof(RemotingSchedulerProxyFactory);
                IRemotableSchedulerProxyFactory factory;
                try
                {
                    factory = ObjectUtils.InstantiateType<IRemotableSchedulerProxyFactory>(proxyType);
                    ObjectUtils.SetObjectProperties(factory, cfg.GetPropertyGroup(PropertySchedulerProxy, true));
                }
                catch (Exception e)
                {
                    initException = new SchedulerException("Remotable proxy factory '{0}' could not be instantiated.".FormatInvariant(proxyType), e);
                    throw initException;
                }

                string uid = QuartzSchedulerResources.GetUniqueIdentifier(schedName, schedInstId);

                RemoteScheduler remoteScheduler = new RemoteScheduler(uid, factory);

                schedRep.Bind(remoteScheduler);

                return remoteScheduler;
            }
            #endregion

            #region job factory

            IJobFactory jobFactory = null;
            Type jobFactoryType = LoadType(cfg.GetStringProperty(PropertySchedulerJobFactoryType, null));

            if (jobFactoryType != null)
            {
                try
                {
                    jobFactory = ObjectUtils.InstantiateType<IJobFactory>(jobFactoryType);
                }
                catch (Exception e)
                {
                    throw new SchedulerConfigException("Unable to Instantiate JobFactory: {0}".FormatInvariant(e.Message), e);
                }

                tProps = cfg.GetPropertyGroup(PropertySchedulerJobFactoryPrefix, true);
                try
                {
                    ObjectUtils.SetObjectProperties(jobFactory, tProps);
                }
                catch (Exception e)
                {
                    initException = new SchedulerException("JobFactory of type '{0}' props could not be configured.".FormatInvariant(jobFactoryType), e);
                    throw initException;
                }
            }

            #endregion

            #region InstanceIdGenerator

            IInstanceIdGenerator instanceIdGenerator = null;
            if (instanceIdGeneratorType != null)
            {
                try
                {
                    instanceIdGenerator = ObjectUtils.InstantiateType<IInstanceIdGenerator>(instanceIdGeneratorType);
                }
                catch (Exception e)
                {
                    throw new SchedulerConfigException("Unable to Instantiate InstanceIdGenerator: {0}".FormatInvariant(e.Message), e);
                }
                tProps = cfg.GetPropertyGroup(PropertySchedulerInstanceIdGeneratorPrefix, true);
                try
                {
                    ObjectUtils.SetObjectProperties(instanceIdGenerator, tProps);
                }
                catch (Exception e)
                {
                    initException = new SchedulerException("InstanceIdGenerator of type '{0}' props could not be configured.".FormatInvariant(instanceIdGeneratorType), e);
                    throw initException;
                }
            }
            #endregion

            #region ThreadPool

            Type tpType = loadHelper.LoadType(cfg.GetStringProperty(PropertyThreadPoolType)) ?? typeof(SimpleThreadPool);

            try
            {
                threadPool = ObjectUtils.InstantiateType<IThreadPool>(tpType);
            }
            catch (Exception e)
            {
                initException = new SchedulerException("ThreadPool type '{0}' could not be instantiated.".FormatInvariant(tpType), e);
                throw initException;
            }
            tProps = cfg.GetPropertyGroup(PropertyThreadPoolPrefix, true);
            try
            {
                ObjectUtils.SetObjectProperties(threadPool, tProps);
            }
            catch (Exception e)
            {
                initException = new SchedulerException("ThreadPool type '{0}' props could not be configured.".FormatInvariant(tpType), e);
                throw initException;
            }
            #endregion

            #region DataSource
            IList<string> dsNames = cfg.GetPropertyGroups(PropertyDataSourcePrefix);
            foreach (string dataSourceName in dsNames)
            {
                string datasourceKey = "{0}.{1}".FormatInvariant(PropertyDataSourcePrefix, dataSourceName);
                NameValueCollection propertyGroup = cfg.GetPropertyGroup(datasourceKey, true);
                PropertiesParser pp = new PropertiesParser(propertyGroup);

                Type dbProviderType = loadHelper.LoadType(pp.GetStringProperty(PropertyDbProviderType, null));

                if (dbProviderType != null)
                {
                    IDbProvider cp;
                    try
                    {
                        cp = ObjectUtils.InstantiateType<IDbProvider>(dbProviderType);
                    }
                    catch (Exception e)
                    {
                        initException = new SchedulerException("ConnectionProvider of type '{0}' could not be instantiated.".FormatInvariant(dbProviderType), e);
                        throw initException;
                    }

                    try
                    {
                        // remove the type name, so it isn't attempted to be set
                        pp.UnderlyingProperties.Remove(PropertyDbProviderType);

                        ObjectUtils.SetObjectProperties(cp, pp.UnderlyingProperties);
                        cp.Initialize();
                    }
                    catch (Exception e)
                    {
                        initException = new SchedulerException("ConnectionProvider type '{0}' props could not be configured.".FormatInvariant(dbProviderType), e);
                        throw initException;
                    }

                    dbMgr = DBConnectionManager.Instance;
                    dbMgr.AddConnectionProvider(dataSourceName, cp);
                }
                else
                {
                    string dsProvider = pp.GetStringProperty(PropertyDataSourceProvider, null);
                    string dsConnectionString = pp.GetStringProperty(PropertyDataSourceConnectionString, null);
                    string dsConnectionStringName = pp.GetStringProperty(PropertyDataSourceConnectionStringName, null);

                    if (dsConnectionString == null && !String.IsNullOrEmpty(dsConnectionStringName))
                    {

                        ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings[dsConnectionStringName];
                        if (connectionStringSettings == null)
                        {
                            initException = new SchedulerException("Named connection string '{0}' not found for DataSource: {1}".FormatInvariant(dsConnectionStringName, dataSourceName));
                            throw initException;
                        }
                        dsConnectionString = connectionStringSettings.ConnectionString;
                    }

                    if (dsProvider == null)
                    {
                        initException = new SchedulerException("Provider not specified for DataSource: {0}".FormatInvariant(dataSourceName));
                        throw initException;
                    }
                    if (dsConnectionString == null)
                    {
                        initException = new SchedulerException("Connection string not specified for DataSource: {0}".FormatInvariant(dataSourceName));
                        throw initException;
                    }
                    try
                    {
                        DbProvider dbp = new DbProvider(dsProvider, dsConnectionString);
                        dbp.Initialize();

                        dbMgr = DBConnectionManager.Instance;
                        dbMgr.AddConnectionProvider(dataSourceName, dbp);
                    }
                    catch (Exception exception)
                    {
                        initException = new SchedulerException("Could not Initialize DataSource: {0}".FormatInvariant(dataSourceName), exception);
                        throw initException;
                    }
                }
            }

            #endregion

            #region Object Serializer
            IObjectSerializer objectSerializer;
            string objectSerializerType = cfg.GetStringProperty("quartz.serializer.type");
            if (objectSerializerType != null)
            {
                tProps = cfg.GetPropertyGroup(PropertyObjectSerializer, true);
                try
                {
                    objectSerializer = ObjectUtils.InstantiateType<IObjectSerializer>(loadHelper.LoadType(objectSerializerType));
                    log.Info("Using custom implementation for object serializer: " + objectSerializerType);

                    ObjectUtils.SetObjectProperties(objectSerializer, tProps);
                }
                catch (Exception e)
                {
                    initException = new SchedulerException("Object serializer type '" + objectSerializerType + "' could not be instantiated.", e);
                    throw initException;
                }
            }
            else
            {
                log.Info("Using default implementation for object serializer");
                objectSerializer = new DefaultObjectSerializer();
            }
            #endregion

            #region JobStore

            Type jobStoreType = loadHelper.LoadType(cfg.GetStringProperty(PropertyJobStoreType));
            try
            {
                jobStore = ObjectUtils.InstantiateType<IJobStore>(jobStoreType ?? typeof(RAMJobStore));
            }
            catch (Exception e)
            {
                initException = new SchedulerException("JobStore of type '{0}' could not be instantiated.".FormatInvariant(jobStoreType), e);
                throw initException;
            }

            SchedulerDetailsSetter.SetDetails(jobStore, schedName, schedInstId);

            tProps = cfg.GetPropertyGroup(PropertyJobStorePrefix, true, new string[] { PropertyJobStoreLockHandlerPrefix });

            try
            {
                ObjectUtils.SetObjectProperties(jobStore, tProps);
            }
            catch (Exception e)
            {
                initException = new SchedulerException("JobStore type '{0}' props could not be configured.".FormatInvariant(jobStoreType), e);
                throw initException;
            }

            JobStoreSupport jobStoreSupport = jobStore as JobStoreSupport;
            if (jobStoreSupport != null)
            {
                Type lockHandlerType = loadHelper.LoadType(cfg.GetStringProperty(PropertyJobStoreLockHandlerType));
                if (lockHandlerType != null)
                {
                    try
                    {
                        ISemaphore lockHandler;
                        ConstructorInfo cWithDbProvider = lockHandlerType.GetConstructor(new Type[] { typeof(DbProvider) });

                        if (cWithDbProvider != null)
                        {
                            // takes db provider
                            IDbProvider dbProvider = DBConnectionManager.Instance.GetDbProvider(jobStoreSupport.DataSource);
                            lockHandler = (ISemaphore)cWithDbProvider.Invoke(new object[] { dbProvider });
                        }
                        else
                        {
                            lockHandler = ObjectUtils.InstantiateType<ISemaphore>(lockHandlerType);
                        }

                        tProps = cfg.GetPropertyGroup(PropertyJobStoreLockHandlerPrefix, true);

                        // If this lock handler requires the table prefix, add it to its properties.
                        if (lockHandler is ITablePrefixAware)
                        {
                            tProps[PropertyTablePrefix] = jobStoreSupport.TablePrefix;
                            tProps[PropertySchedulerName] = schedName;
                        }

                        try
                        {
                            ObjectUtils.SetObjectProperties(lockHandler, tProps);
                        }
                        catch (Exception e)
                        {
                            initException = new SchedulerException("JobStore LockHandler type '{0}' props could not be configured.".FormatInvariant(lockHandlerType), e);
                            throw initException;
                        }

                        jobStoreSupport.LockHandler = lockHandler;
                        Log.Info("Using custom data access locking (synchronization): " + lockHandlerType);
                    }
                    catch (Exception e)
                    {
                        initException = new SchedulerException("JobStore LockHandler type '{0}' could not be instantiated.".FormatInvariant(lockHandlerType), e);
                        throw initException;
                    }
                }
            }
            #endregion

            #region SchedulerPlugins

            IList<string> pluginNames = cfg.GetPropertyGroups(PropertyPluginPrefix);
            ISchedulerPlugin[] plugins = new ISchedulerPlugin[pluginNames.Count];
            for (int i = 0; i < pluginNames.Count; i++)
            {
                NameValueCollection pp = cfg.GetPropertyGroup("{0}.{1}".FormatInvariant(PropertyPluginPrefix, pluginNames[i]), true);

                string plugInType = pp[PropertyPluginType];

                if (plugInType == null)
                {
                    initException = new SchedulerException("SchedulerPlugin type not specified for plugin '{0}'".FormatInvariant(pluginNames[i]));
                    throw initException;
                }
                ISchedulerPlugin plugin;
                try
                {
                    plugin = ObjectUtils.InstantiateType<ISchedulerPlugin>(LoadType(plugInType));
                }
                catch (Exception e)
                {
                    initException = new SchedulerException("SchedulerPlugin of type '{0}' could not be instantiated.".FormatInvariant(plugInType), e);
                    throw initException;
                }
                try
                {
                    ObjectUtils.SetObjectProperties(plugin, pp);
                }
                catch (Exception e)
                {
                    initException = new SchedulerException("JobStore SchedulerPlugin '{0}' props could not be configured.".FormatInvariant(plugInType), e);
                    throw initException;
                }
                plugins[i] = plugin;
            }
            #endregion

            #region JobListeners
            IList<string> jobListenerNames = cfg.GetPropertyGroups(PropertyJobListenerPrefix);
            IJobListener[] jobListeners = new IJobListener[jobListenerNames.Count];
            for (int i = 0; i < jobListenerNames.Count; i++)
            {
                NameValueCollection lp = cfg.GetPropertyGroup("{0}.{1}".FormatInvariant(PropertyJobListenerPrefix, jobListenerNames[i]), true);

                string listenerType = lp[PropertyListenerType];

                if (listenerType == null)
                {
                    initException = new SchedulerException("JobListener type not specified for listener '{0}'".FormatInvariant(jobListenerNames[i]));
                    throw initException;
                }
                IJobListener listener;
                try
                {
                    listener = ObjectUtils.InstantiateType<IJobListener>(loadHelper.LoadType(listenerType));
                }
                catch (Exception e)
                {
                    initException = new SchedulerException("JobListener of type '{0}' could not be instantiated.".FormatInvariant(listenerType), e);
                    throw initException;
                }
                try
                {
                    PropertyInfo nameProperty = listener.GetType().GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
                    if (nameProperty != null && nameProperty.CanWrite)
                    {
                        nameProperty.GetSetMethod().Invoke(listener, new object[] { jobListenerNames[i] });
                    }
                    ObjectUtils.SetObjectProperties(listener, lp);
                }
                catch (Exception e)
                {
                    initException = new SchedulerException("JobListener '{0}' props could not be configured.".FormatInvariant(listenerType), e);
                    throw initException;
                }
                jobListeners[i] = listener;
            }
            #endregion

            #region TriggerListener
            IList<string> triggerListenerNames = cfg.GetPropertyGroups(PropertyTriggerListenerPrefix);
            ITriggerListener[] triggerListeners = new ITriggerListener[triggerListenerNames.Count];
            for (int i = 0; i < triggerListenerNames.Count; i++)
            {
                NameValueCollection lp = cfg.GetPropertyGroup("{0}.{1}".FormatInvariant(PropertyTriggerListenerPrefix, triggerListenerNames[i]), true);

                string listenerType = lp[PropertyListenerType];

                if (listenerType == null)
                {
                    initException = new SchedulerException("TriggerListener type not specified for listener '{0}'".FormatInvariant(triggerListenerNames[i]));
                    throw initException;
                }
                ITriggerListener listener;
                try
                {
                    listener = ObjectUtils.InstantiateType<ITriggerListener>(loadHelper.LoadType(listenerType));
                }
                catch (Exception e)
                {
                    initException = new SchedulerException("TriggerListener of type '{0}' could not be instantiated.".FormatInvariant(listenerType), e);
                    throw initException;
                }
                try
                {
                    PropertyInfo nameProperty = listener.GetType().GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
                    if (nameProperty != null && nameProperty.CanWrite)
                    {
                        nameProperty.GetSetMethod().Invoke(listener, new object[] { triggerListenerNames[i] });
                    }
                    ObjectUtils.SetObjectProperties(listener, lp);
                }
                catch (Exception e)
                {
                    initException = new SchedulerException("TriggerListener '{0}' props could not be configured.".FormatInvariant(listenerType), e);
                    throw initException;
                }
                triggerListeners[i] = listener;
            }
            #endregion

            #region Exporter
            string exporterType = cfg.GetStringProperty(PropertySchedulerExporterType, null);

            if (exporterType != null)
            {
                try
                {
                    exporter = ObjectUtils.InstantiateType<ISchedulerExporter>(loadHelper.LoadType(exporterType));
                }
                catch (Exception e)
                {
                    initException = new SchedulerException("Scheduler exporter of type '{0}' could not be instantiated.".FormatInvariant(exporterType), e);
                    throw initException;
                }

                tProps = cfg.GetPropertyGroup(PropertySchedulerExporterPrefix, true);

                try
                {
                    ObjectUtils.SetObjectProperties(exporter, tProps);
                }
                catch (Exception e)
                {
                    initException = new SchedulerException("Scheduler exporter type '{0}' props could not be configured.".FormatInvariant(exporterType), e);
                    throw initException;
                }
            }
            #endregion

            bool tpInited = false;
            bool qsInited = false;

            #region Thread Executor

            string threadExecutorClass = cfg.GetStringProperty(PropertyThreadExecutorType);
            if (threadExecutorClass != null)
            {
                tProps = cfg.GetPropertyGroup(PropertyThreadExecutor, true);
                try
                {
                    threadExecutor = ObjectUtils.InstantiateType<IThreadExecutor>(loadHelper.LoadType(threadExecutorClass));
                    log.Info("Using custom implementation for ThreadExecutor: " + threadExecutorClass);

                    ObjectUtils.SetObjectProperties(threadExecutor, tProps);
                }
                catch (Exception e)
                {
                    initException = new SchedulerException(
                            "ThreadExecutor class '" + threadExecutorClass + "' could not be instantiated.", e);
                    throw initException;
                }
            }
            else
            {
                log.Info("Using default implementation for ThreadExecutor");
                threadExecutor = new DefaultThreadExecutor();
            }
            #endregion

            #region Fire Up
            try
            {
                IJobRunShellFactory shellFactory = new StdJobRunShellFactory();

                if (autoId)
                {
                    try
                    {
                        schedInstId = DefaultInstanceId;

                        if (jobStore.Clustered)
                        {
                            schedInstId = instanceIdGenerator.GenerateInstanceId();
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error("Couldn't generate instance Id!", e);
                        throw new SystemException("Cannot run without an instance id.");
                    }
                }

                jobStoreSupport = jobStore as JobStoreSupport;
                if (jobStoreSupport != null)
                {
                    jobStoreSupport.DbRetryInterval = dbFailureRetry;
                    jobStoreSupport.ThreadExecutor = threadExecutor;
                    // object serializer
                    jobStoreSupport.ObjectSerializer = objectSerializer;
                }

                QuartzSchedulerResources rsrcs = new QuartzSchedulerResources();
                rsrcs.Name = schedName;
                rsrcs.ThreadName = threadName;
                rsrcs.InstanceId = schedInstId;
                rsrcs.JobRunShellFactory = shellFactory;
                rsrcs.MakeSchedulerThreadDaemon = makeSchedulerThreadDaemon;
                rsrcs.BatchTimeWindow = TimeSpan.FromMilliseconds(batchTimeWindow);
                rsrcs.MaxBatchSize = maxBatchSize;
                rsrcs.InterruptJobsOnShutdown = interruptJobsOnShutdown;
                rsrcs.InterruptJobsOnShutdownWithWait = interruptJobsOnShutdownWithWait;
                rsrcs.SchedulerExporter = exporter;

                SchedulerDetailsSetter.SetDetails(threadPool, schedName, schedInstId);

                rsrcs.ThreadExecutor = threadExecutor;
                threadExecutor.Initialize();

                rsrcs.ThreadPool = threadPool;

                threadPool.Initialize();
                tpInited = true;

                rsrcs.JobStore = jobStore;

                // add plugins
                foreach (ISchedulerPlugin plugin in plugins)
                {
                    rsrcs.AddSchedulerPlugin(plugin);
                }

                qs = new QuartzScheduler(rsrcs, idleWaitTime);
                qsInited = true;

                IScheduler sched = Instantiate(rsrcs, qs);

                // set job factory if specified
                if (jobFactory != null)
                {
                    qs.JobFactory = jobFactory;
                }

                // Initialize plugins now that we have a Scheduler instance.
                for (int i = 0; i < plugins.Length; i++)
                {
                    plugins[i].Initialize(pluginNames[i], sched);
                }

                // add listeners
                foreach (IJobListener listener in jobListeners)
                {
                    qs.ListenerManager.AddJobListener(listener, EverythingMatcher<JobKey>.AllJobs());
                }
                foreach (ITriggerListener listener in triggerListeners)
                {
                    qs.ListenerManager.AddTriggerListener(listener, EverythingMatcher<TriggerKey>.AllTriggers());
                }

                // set scheduler context data...
                foreach (string key in schedCtxtProps)
                {
                    string val = schedCtxtProps.Get(key);
                    sched.Context.Put(key, val);
                }

                // fire up job store, and runshell factory

                jobStore.InstanceId = schedInstId;
                jobStore.InstanceName = schedName;
                jobStore.ThreadPoolSize = threadPool.PoolSize;
                jobStore.Initialize(loadHelper, qs.SchedulerSignaler);

                shellFactory.Initialize(sched);
                qs.Initialize();

                Log.Info("Quartz scheduler '{0}' initialized".FormatInvariant(sched.SchedulerName));

                Log.Info("Quartz scheduler version: {0}".FormatInvariant(qs.Version));

                // prevents the repository from being garbage collected
                qs.AddNoGCObject(schedRep);
                // prevents the db manager from being garbage collected
                if (dbMgr != null)
                {
                    qs.AddNoGCObject(dbMgr);
                }

                schedRep.Bind(sched);

                return sched;
            }
            catch (SchedulerException)
            {
                ShutdownFromInstantiateException(threadPool, qs, tpInited, qsInited);
                throw;
            }
            catch
            {
                ShutdownFromInstantiateException(threadPool, qs, tpInited, qsInited);
                throw;
            }
            #endregion
        }






        private void ShutdownFromInstantiateException(IThreadPool tp, QuartzScheduler qs, bool tpInited, bool qsInited)
        {
            try
            {
                if (qsInited)
                {
                    qs.Shutdown(false);
                }
                else if (tpInited)
                {
                    tp.Shutdown(false);
                }
            }
            catch (Exception e)
            {
                Log.Error("Got another exception while shutting down after instantiation exception", e);
            }
        }

        protected virtual IScheduler Instantiate(QuartzSchedulerResources rsrcs, QuartzScheduler qs)
        {
            IScheduler sched = new StdScheduler(qs);
            return sched;
        }

        protected virtual Type LoadType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            return Type.GetType(typeName, true);
        }

        public virtual IScheduler GetScheduler()
        {
            if (cfg == null)
            {
                Initialize();
            }

            SchedulerRepository schedRep = SchedulerRepository.Instance;

            IScheduler sched = schedRep.Lookup(SchedulerName);

            if (sched != null)
            {
                if (sched.IsShutdown)
                {
                    schedRep.Remove(SchedulerName);
                }
                else
                {
                    return sched;
                }
            }

            sched = Instantiate();

            return sched;
        }

        public virtual IScheduler GetScheduler(string schedName)
        {
            return SchedulerRepository.Instance.Lookup(schedName);
        }
    }
}