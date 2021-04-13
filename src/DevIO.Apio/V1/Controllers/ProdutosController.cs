using AutoMapper;
using DevIO.Apio.Controllers;
using DevIO.Apio.ViewModels;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DevIO.Apio.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class ProdutosController : MainController
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IProdutoService _produtoService;
        private readonly IMapper _mapper;
        public ProdutosController(INotificador notificador,
                                  IProdutoRepository produtoRepository,
                                  IProdutoService produtoService,
                                  IMapper mapper,
                                  IUser user) : base(notificador, user)
        {
            _produtoRepository = produtoRepository;
            _produtoService = produtoService;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> ObterTodos()
        {
            return Ok(_mapper.Map<IEnumerable<ProdutoViewModel>>(await _produtoRepository.ObterProdutosFornecedores()));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> ObterPorId([FromRoute] Guid id)
        {
            var produtoViewModel = await ObterProduto(id);

            if (produtoViewModel == null)
            {
                return NotFound();
            }

            return produtoViewModel;
        }

        [HttpPost]
        public async Task<ActionResult<ProdutoViewModel>> Adicionar([FromForm] ProdutoImagemViewModel produto)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var imgPrefixo = Guid.NewGuid() + "_" + produto.ImagemUpload.Name;

            if (!await UploadArquivoAlternativo(produto.ImagemUpload, imgPrefixo))
            {
                return CustomResponse(produto);
            }

            produto.Imagem = imgPrefixo;

            await _produtoService.Adicionar(_mapper.Map<Produto>(produto));

            return CustomResponse(produto);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> Excluir([FromRoute] Guid id)
        {
            var produtoViewModel = await ObterProduto(id);

            if (produtoViewModel == null)
            {
                return NotFound();
            }

            await _produtoService.Remover(id);

            return CustomResponse(produtoViewModel);
        }

        private async Task<ProdutoViewModel> ObterProduto(Guid id)
        {
            return _mapper.Map<ProdutoViewModel>(await _produtoRepository.ObterPorId(id));
        }

        private bool UploadArquivo(string arquivo, string imgNome)
        {
            if (string.IsNullOrEmpty(arquivo))
            {
                ModelState.AddModelError(string.Empty, "Forneça uma imagem para este produto");
                return false;
            }

            var imageDataByteArray = Convert.FromBase64String(arquivo);

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imagens", imgNome);

            if (System.IO.File.Exists(filePath))
            {
                ModelState.AddModelError(string.Empty, "Já existe um arquivo com este nome");
                return false;
            }

            System.IO.File.WriteAllBytes(filePath, imageDataByteArray);

            return true;
        }

        private async Task<bool> UploadArquivoAlternativo(IFormFile arquivo, string imgPrefixo)
        {
            if (arquivo == null || arquivo.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Forneça uma imagem para este produto");
                return false;
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imagens", imgPrefixo + arquivo.FileName);

            if (System.IO.File.Exists(filePath))
            {
                ModelState.AddModelError(string.Empty, "Já existe um arquivo com este nome");
                return false;
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await arquivo.CopyToAsync(stream);
            }

            return true;
        }
    }
}
