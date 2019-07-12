using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Redis
{
    public interface IRedis
    {
        /// <summary>
        /// 设置key-value值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        bool StringSet(string key, string value, TimeSpan time);


        /// <summary>
        /// 获取key对应的value值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string StringGet(string key);

        /// <summary>
        /// 获取key对应的value值
        /// </summary>
        /// <param name="keyHead"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        string StringGet(string keyHead, string key);

        /// <summary>
        /// 验证key是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool KeyExists(string key);

        /// <summary>
        /// 刷新keys有效时间
        /// </summary>
        /// <param name="keyList"></param>
        /// <param name="time"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool RefreshKeysTTL(List<string> keyList, TimeSpan time, out string msg);

        /// <summary>
        /// 清除keys
        /// </summary>
        /// <param name="keyList"></param>
        /// <returns></returns>
        bool RemoveKeys(List<string> keyList);


        #region //消息队列
        /// <summary>
        /// 消息队列执行状态-开始
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        bool QueueExeBegin(string keyName);
        /// <summary>
        ///  消息队列执行状态-结束
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        bool QueueExeEnd(string keyName);
        /// <summary>
        /// 消息队列执行状态
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        string QueueExeState(string keyName);
        /// <summary>
        /// 消息队列-追加任务
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="queueValue"></param>
        /// <returns></returns>
        bool QueuePush(string queueName, string queueValue);
        /// <summary>
        /// 消息队列-消费之前，放入暂存队列（每次唯一）
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="tempName"></param>
        /// <param name="queueValue"></param>
        /// <returns></returns>
        bool QueuePushToTemp(string queueName, string tempName, out string queueValue);
        /// <summary>
        /// 消息队列-消费失败，从暂存队列弹回任务队列
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="tempName"></param>
        /// <param name="queueValue"></param>
        /// <returns></returns>
        bool QueuePopTempBack(string queueName, string tempName, out string queueValue);
        /// <summary>
        /// 消息队列-消费成功，移除暂存队列
        /// </summary>
        /// <param name="tempName"></param>
        /// <param name="queueValue"></param>
        /// <returns></returns>
        bool QueuePop(string tempName, out string queueValue);
        #endregion
    }
}
