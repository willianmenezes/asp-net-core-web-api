using DevIO.Business.Intefaces;
using DevIO.Business.Notificacoes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Linq;

namespace DevIO.Apio.Controllers
{
    [ApiController]
    public abstract class MainController : ControllerBase
    {
        private readonly INotificador _notificador;
        public readonly IUser _appUser;

        protected Guid UsuarioId { get; set; }
        protected bool UsuarioAuthenticado { get; set; }

        protected MainController(INotificador notificador, IUser appUser)
        {
            _notificador = notificador;
            _appUser = appUser;

            if (_appUser.IsAuthenticated())
            {
                UsuarioId = _appUser.GetUserId();
                UsuarioAuthenticado = true;
            }
        }

        protected bool OperacaoValida()
        {
            return !_notificador.TemNotificacao();
        }

        protected ActionResult CustomResponse(ModelStateDictionary modelState)
        {
            if (!modelState.IsValid)
            {
                NotificarErroModelInvalida(modelState);
            }

            return CustomResponse();
        }

        protected ActionResult CustomResponse(object result = null)
        {
            if (OperacaoValida())
            {
                return Ok(new
                {
                    sucess = true,
                    data = result,
                });

            }

            return BadRequest(new
            {
                success = false,
                erros = _notificador.ObterNotificacoes().Select(m => m.Mensagem)
            });
        }

        protected void NotificarErroModelInvalida(ModelStateDictionary modelState)
        {
            var erros = modelState.Values.SelectMany(e => e.Errors);

            foreach (var erro in erros)
            {
                var errorMessage = erro.Exception == null ? erro.ErrorMessage : erro.Exception.Message;

                NotificarErro(errorMessage);
            }
        }

        protected void NotificarErro(string mensagem)
        {
            _notificador.Handle(new Notificacao(mensagem));
        }
    }
}
