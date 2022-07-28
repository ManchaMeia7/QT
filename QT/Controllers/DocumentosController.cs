using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QT.DataB;
using QT.Models;

namespace QT.Controllers
{
    public class DocumentosController : Controller
    {
        private readonly Contexto _context;
        private readonly INotyfService _notyf;
        private string[] ExtensoesValidas = { ".pdf", ".doc", ".xls", ".docx", ".xlsx" };

        public DocumentosController(Contexto context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }
        public bool VerificaSeODocumentoExiste(int codigoDoDocumento)
        {
            var documento = _context.Documentos.AsNoTracking()
                .Where(documento => documento.Codigo == codigoDoDocumento).ToList();

            return documento.Any();
        }
        private bool VerificaExtensaoDoArquivo(string extensaoDoArquivo)
            => ExtensoesValidas.Contains(extensaoDoArquivo);

        public async Task<IActionResult> Index()
        {
            var contexto = _context.Documentos.OrderBy(d => d.Titulo).Include(d => d.Processo);
            return View(await contexto.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Documentos == null)
            {
                return NotFound();
            }

            var documento = await _context.Documentos
                .Include(d => d.Processo)
                .FirstOrDefaultAsync(m => m.Codigo == id);
            if (documento == null)
            {
                return NotFound();
            }

            return View(documento);
        }

        public IActionResult Create()
        {
            ViewData["ProcessoCodigo"] = new SelectList(_context.Processos, "Codigo", "Nome");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Documento documento, IFormFile Arquivo)
        {
            ViewData["ProcessoCodigo"] = new SelectList(_context.Processos, "Codigo", "Nome", documento.ProcessoCodigo);

            ModelState.Remove("Processo");
            ModelState.Remove("Arquivo");
            ModelState.Remove("ExtensaoDoArquivo");
            ModelState.Remove("NomeDoArquivo");
            if (ModelState.IsValid && Arquivo != null)
            {
                try
                {
                    var nomeDoArquivo = Path.GetFileName(Arquivo.FileName);
                    documento.NomeDoArquivo = nomeDoArquivo;
                    var extensaoDoArquivo = Path.GetExtension(nomeDoArquivo);
                    if (VerificaExtensaoDoArquivo(extensaoDoArquivo))
                    {
                        var documentoExiste = VerificaSeODocumentoExiste(documento.Codigo);

                        using (var memoryStream = new MemoryStream())
                        {
                            Arquivo.CopyTo(memoryStream);
                            documento.Arquivo = memoryStream.ToArray();
                        }

                        if (!documentoExiste)
                        {
                            _context.Add(documento);
                            await _context.SaveChangesAsync();
                            _notyf.Success("Novo documento cadastrado.");
                            return RedirectToAction(nameof(Index));
                        }
                        else
                            _notyf.Error("Código em uso.");
                    }
                    else
                        _notyf.Error("Extensão do arquivo não é válida!");
                }
                catch
                {
                    _notyf.Error("Ocorreu um erro ao cadastrar o documento. Tente novamente!");
                }
            }
            else
                _notyf.Error("Ocorreu um erro ao cadastrar o documento. Tente novamente!");

            return View(documento);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Documentos == null)
            {
                return NotFound();
            }

            var documento = await _context.Documentos.FindAsync(id);
            if (documento == null)
            {
                return NotFound();
            }
            ViewData["ProcessoCodigo"] = new SelectList(_context.Processos, "Codigo", "Nome", documento.ProcessoCodigo);
            return View(documento);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Codigo,Titulo,ProcessoCodigo,Categoria")] Documento documento, IFormFile Arquivo)
        {
            if (id != documento.Codigo)
            {
                return NotFound();
            }

            ModelState.Remove("Processo");
            ModelState.Remove("Arquivo");
            ModelState.Remove("ExtensaoDoArquivo");
            ModelState.Remove("NomeDoArquivo");
            if (ModelState.IsValid && Arquivo != null)
            {
                try
                {
                    var nomeDoArquivo = Path.GetFileName(Arquivo.FileName);
                    documento.NomeDoArquivo = nomeDoArquivo;
                    var extensaoDoArquivo = Path.GetExtension(nomeDoArquivo);
                    if (VerificaExtensaoDoArquivo(extensaoDoArquivo))
                    {
                        var documentoExiste = VerificaSeODocumentoExiste(documento.Codigo);

                        using (var memoryStream = new MemoryStream())
                        {
                            Arquivo.CopyTo(memoryStream);
                            documento.Arquivo = memoryStream.ToArray();
                        }
                    }

                    _context.Update(documento);
                    await _context.SaveChangesAsync();
                    _notyf.Success("Edição feita com sucesso.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DocumentoExists(documento.Codigo))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProcessoCodigo"] = new SelectList(_context.Processos, "Codigo", "Nome", documento.ProcessoCodigo);
            return View(documento);
        }

        public async Task<IActionResult> Baixar(int id)
        {
            try
            {
                var documento = _context.Documentos
                    .Where(documento => documento.Codigo == id).FirstOrDefault();
                string nomeDoArquivo = documento.NomeDoArquivo;
                byte[] arquivo = documento.Arquivo;

                return File(arquivo, "application/octet-stream", nomeDoArquivo);
            }
            catch
            {
                _notyf.Error("Ocorreu um erro ao baixar o arquivo. Tente novamente!");

                return View();
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Documentos == null)
            {
                return NotFound();
            }

            var documento = await _context.Documentos
                .Include(d => d.Processo)
                .FirstOrDefaultAsync(m => m.Codigo == id);
            if (documento == null)
            {
                return NotFound();
            }

            return View(documento);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Documentos == null)
            {
                return Problem("Entity set 'Contexto.Documentos'  is null.");
            }
            var documento = await _context.Documentos.FindAsync(id);
            if (documento != null)
            {
                _context.Documentos.Remove(documento);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DocumentoExists(int id)
        {
          return (_context.Documentos?.Any(e => e.Codigo == id)).GetValueOrDefault();
        }
    }
}
