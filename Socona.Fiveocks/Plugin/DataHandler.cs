﻿using Socona.Fiveocks.TCP;

namespace Socona.Fiveocks.Plugin
{
    public abstract class DataHandler : PluginBase
    {
        /// <summary>
        /// Allows you to grab data before it's sent to the end user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public abstract void OnDataReceived(object sender, DataEventArgs e);

        /// <summary>
        /// Allows you to grab/modify data before it's sent to the end server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public abstract void OnDataSent(object sender, DataEventArgs e);


    }
}
