/*
 * Copyright (c) 1998-2016 Caucho Technology -- all rights reserved
 *
 * This file is part of Resin(R) Open Source
 *
 * Each copy or derived work must preserve the copyright notice and this
 * notice unmodified.
 *
 * Resin Open Source is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License version 2
 * as published by the Free Software Foundation.
 *
 * Resin Open Source is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE, or any warranty
 * of NON-INFRINGEMENT.  See the GNU General Public License for more
 * details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Resin Open Source; if not, write to the
 *
 *   Free Software Foundation, Inc.
 *   59 Temple Place, Suite 330
 *   Boston, MA 02111-1307  USA
 *
 * @author Alex Rojkov
 */
using System;
using System.Reflection;
using System.Collections;
using System.Text;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;

namespace Caucho
{
  public class Nginx : ServiceBase
  {

    private Mutex _mutex;

    private Nginx()
    {
      if (_mutex == null) {
          _mutex = new Mutex(false, @"nginx");
      }
    }

    public bool StartNginx()
    {
      try
      {
      _mutex.WaitOne();
      try
      {
        ExecuteStartImpl();
      }
      finally
      {
        _mutex.ReleaseMutex();
      }

        return true;
      } catch (NginxServiceException e)
      {
        throw e;
      } catch (Exception e)
      {
        return false;
      }
    }

    public void StopNginx()
    {
      _mutex.WaitOne();
      try
      {
        ExecuteStopImpl();
      }
      finally
      {
        _mutex.ReleaseMutex();
      }
    }

    private int Execute()
    {
        ServiceBase.Run(new ServiceBase[] { this });
        return 0;
    }

    private void ExecuteStartImpl()
    {
      ProcessStartInfo startInfo = new ProcessStartInfo();
      startInfo.FileName = "nginx.exe";
      startInfo.WorkingDirectory = "C:\\nginx\\";
      //startInfo.Arguments = "-c conf\\nginx.conf";
      startInfo.UseShellExecute = false;
        startInfo.RedirectStandardError = true;
        startInfo.RedirectStandardOutput = true;

        Process process = null;
        try
        {
          process = Process.Start(startInfo);
        } catch (Exception e)
        {
          return;
        }

    }

    private void ExecuteStopImpl()
    {
      ProcessStartInfo startInfo = new ProcessStartInfo();
      startInfo.FileName = "taskkill.exe";
      startInfo.Arguments = "/IM nginx.exe /F";
      startInfo.UseShellExecute = false;
        startInfo.RedirectStandardError = true;
        startInfo.RedirectStandardOutput = true;

        Process process = null;
        try
        {
          process = Process.Start(startInfo);
        } catch (Exception e)
        {
          return;
        }

    }

    public static int Main(String[] args)
    {
      Nginx nginx = new Nginx();

      return nginx.Execute();
    }

    protected override void OnStart(string[] args)
    {
      base.OnStart(args);

      StartNginx();
    }

    protected override void OnStop()
    {
      base.OnStop();

      StopNginx();
    }

  }
}

class NginxServiceException : Exception
{
  public NginxServiceException(String message)
    : base(message)
  {
  }
}
