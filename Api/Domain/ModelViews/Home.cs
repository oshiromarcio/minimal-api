using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace minimal_api.Domain.ModelViews
{
    public struct Home
    {
        public string Mensagem { get => "Bem-vindo à API de veículos - Minimal API"; }
        public string Doc { get => "/swagger"; }
    }
}