using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DateTimeService
{
    public partial class DateTimeService : ServiceBase
    {
        Servicio servicio = new Servicio();

        public DateTimeService()
        {
            InitializeComponent();
        }
       
        protected override void OnStart(string[] args)
        {
            servicio.Finaliza = false;
            Thread hilo = new Thread(servicio.Init);
            hilo.Start();
        }

        protected override void OnStop()
        {
            servicio.Finaliza = true;
        }
    }
}