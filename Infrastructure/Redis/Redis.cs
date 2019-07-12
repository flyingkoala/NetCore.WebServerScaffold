using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using Infrastructure.Config;
using Infrastructure.Log;

namespace Infrastructure.Redis
{
    public class Redis : IRedis
    {
        protected static ConnectionMultiplexer _connMultiplexer;
        private static readonly object Locker = new object();
        private static ConfigService _service;

        protected readonly IDatabase _redisClient;
        protected readonly IDatabase _redisQueue;

        protected readonly string _keyHead;
        public Redis(ConfigService service)
        {
            string connect = service.config.redis.conn;
            _keyHead = service.config.redis.keyprefix;

            if (!string.IsNullOrEmpty(connect))
            {
                try
                {

                    _redisClient = ConnectionMultiplexer.Connect(connect).GetDatabase();
                    _redisQueue = ConnectionMultiplexer.Connect(connect).GetDatabase(1);
                }
                catch (Exception ex)
                {
                    LogHelper.Log.Error($"conn=>'{connect}'ex=>{ex.Message}");
                }
            }
        }
        private Lazy<ConnectionMultiplexer> _redisConn = new Lazy<ConnectionMultiplexer>(() =>
        {
            if ((_connMultiplexer == null) || !_connMultiplexer.IsConnected)
            {
                lock (Locker)
                {
                    if ((_connMultiplexer == null) || !_connMultiplexer.IsConnected)
                        _connMultiplexer = ConnectionMultiplexer.Connect(_service.config.redis.conn);
                }
            }

            return _connMultiplexer;

            //return ConnectionMultiplexer.Connect(_service.RedisConfig.SlaveServer);

        });

        /// <summary>
        /// 设置key-value值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool StringSet(string key, string value, TimeSpan time)
        {
            return _redisClient.StringSet(string.Concat(_keyHead, key), value, time);
        }


        /// <summary>
        /// 获取key对应的value值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string StringGet(string key)
        {
            return _redisClient.StringGet(string.Concat(_keyHead, key));
        }

        /// <summary>
        /// 获取key对应的value值
        /// </summary>
        /// <param name="keyHead"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string StringGet(string keyHead, string key)
        {
            return _redisClient.StringGet(string.Concat(keyHead, key));
        }

        /// <summary>
        /// 验证key是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool KeyExists(string key)
        {
            return _redisClient.KeyExists(string.Concat(_keyHead, key));
        }

        /// <summary>
        /// 刷新Keys有效时间
        /// </summary>
        /// <param name="keyList"></param>
        /// <param name="time"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool RefreshKeysTTL(List<string> keyList, TimeSpan time, out string msg)
        {
            msg = "0";
            if (keyList == null || keyList.Count == 0)
            {
                msg = "keylist is empty";
                return false;
            }

            bool res = false;
            string tempKey = string.Empty;
            foreach (string key in keyList)
            {
                tempKey = string.Concat(_keyHead, key);
                if (_redisClient.KeyExists(tempKey))
                {
                    res = _redisClient.KeyExpire(tempKey, time);
                    if (!res)
                    {
                        msg = $"redis刷新session失败, key = {key}";
                    }
                }
                else
                    msg = $"redis刷新session失败, key = {key} 不存在";
            }
            return (msg == "0" && res) ? true : false;
        }

        /// <summary>
        /// 清除keys
        /// </summary>
        /// <param name="keyList"></param>
        /// <returns></returns>
        public bool RemoveKeys(List<string> keyList)
        {
            if (keyList == null || keyList.Count == 0) return true;
            bool isOk = false;
            foreach (var key in keyList)
            {
                isOk = _redisClient.KeyDelete($"{_keyHead}{key}");
            }
            return isOk;
        }

        #region //消息队列
        /// <summary>
        /// 消息队列执行状态-开始
        /// </summary>
        /// <returns></returns>
        public bool QueueExeBegin(string keyName)
        {
            return _redisQueue.StringSet($"{_keyHead}{keyName}", 0);
        }

        /// <summary>
        /// 消息队列执行状态-结束
        /// </summary>
        /// <returns></returns>
        public bool QueueExeEnd(string keyName)
        {
            return _redisQueue.StringSet($"{_keyHead}{keyName}", 1);
        }

        /// <summary>
        /// 消息队列执行状态
        /// </summary>
        /// <returns></returns>
        public string QueueExeState(string keyName)
        {
            if (_redisQueue.KeyExists($"{_keyHead}{keyName}"))
                return _redisQueue.StringGet($"{_keyHead}{keyName}");
            else
                return "1";
        }

        /// <summary>
        /// 消息队列-追加任务
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="queueValue"></param>
        /// <returns></returns>
        public bool QueuePush(string queueName, string queueValue)
        {
            try
            {
                long task_id = _redisQueue.ListLeftPush($"{_keyHead}{queueName}", queueValue);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 消息队列-消费之前，放入暂存队列（每次唯一）
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="tempName"></param>
        /// <param name="queueValue"></param>
        /// <returns></returns>
        public bool QueuePushToTemp(string queueName, string tempName, out string queueValue)
        {
            try
            {
                queueValue = _redisQueue.ListRightPopLeftPush($"{_keyHead}{queueName}", $"{_keyHead}{tempName}");
                return true;
            }
            catch (Exception ex)
            {
                queueValue = ex.ToString();
                return false;
            }
        }

        /// <summary>
        /// 消息队列-消费失败，从暂存队列弹回任务队列
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="tempName"></param>
        /// <param name="queueValue"></param>
        /// <returns></returns>
        public bool QueuePopTempBack(string queueName, string tempName, out string queueValue)
        {
            try
            {
                queueValue = _redisQueue.ListRightPopLeftPush($"{_keyHead}{tempName}", $"{_keyHead}{queueName}");
                return true;
            }
            catch (Exception ex)
            {
                queueValue = ex.ToString();
                return false;
            }
        }

        /// <summary>
        /// 消息队列-消费成功，移除暂存队列
        /// </summary>
        /// <param name="tempName"></param>
        /// <param name="queueValue"></param>
        /// <returns></returns>
        public bool QueuePop(string tempName, out string queueValue)
        {
            try
            {
                queueValue = _redisQueue.ListRightPop($"{_keyHead}{tempName}");
                return true;
            }
            catch (Exception ex)
            {
                queueValue = ex.ToString();
                return false;
            }
        }
        #endregion
    }
}