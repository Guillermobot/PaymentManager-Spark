using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PaymentManager.Models;

namespace PaymentManager.Controllers
{

    public class PaymentsController : Controller
    {
        private readonly PaymentDbContext _context;
        private readonly ILogger<PaymentsController> _logger;


        public PaymentsController(PaymentDbContext context, ILogger<PaymentsController> logger)
        {
            _context = context;
            _logger = logger;
        }


         // En lo personal me siento comodo manejando errores con try - catch, me sirve para tener una idea mas clara de los posibles errores y en caso de uno, poder identificar el area de mejora rapidamente


        // GET: Obtener la lista de todos los pagos ordenados del mas reciente al menos
        public async Task<IActionResult> Index()
        {
            try
            {
                var payments = await _context.Payments
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync();
                return View(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar los pagos");
                TempData["ErrorMessage"] = "Error al cargar la lista de pagos";
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Mostrar formulario para crear un nuevo pago 
        public IActionResult Create()
        {
            return View();
        }






        // POST: Guardar un nuevo pago en la base de datos
        //En este veo necesario usar un try catch para manejar los errores y validar si los datos estan presentes antes de iniciar el proceso de guardar en base de datos.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,Amount,PaymentDate,Category,Reference,CreatedAt")] Payment payment)
        {
            // Validar que existan los datos necesarios
            if (string.IsNullOrWhiteSpace(payment.Description) ||
                string.IsNullOrWhiteSpace(payment.Category) ||
                payment.PaymentDate == default ||
                payment.Amount <= 0)
            {
                ModelState.AddModelError("", "Todos los campos requeridos deben estar completos y válidos.");
                return View(payment); // Retorna inmediatamente si falta algún dato
            }

            // Validar que los datos sean correctos
            if (payment.PaymentDate > DateTime.Now)
            {
                ModelState.AddModelError("PaymentDate", "La fecha no puede ser futura.");
            }

            if (!ModelState.IsValid)
            {
                return View(payment);
            }

            // Si todo es valido podemos iniciar el proceso
            try
            {
                // Validación de duplicados
                if (await _context.Payments.AnyAsync(p =>
                    p.Reference == payment.Reference &&
                    p.Category == payment.Category &&
                    p.Amount == payment.Amount))
                {
                    ModelState.AddModelError("", "Este pago ya existe.");
                    return View(payment);
                }

                payment.CreatedAt = DateTime.Now;
                _context.Add(payment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "¡Pago registrado exitosamente!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error de base de datos al guardar el pago");
                ModelState.AddModelError("", "Error al guardar. Por favor, intente nuevamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado");
                TempData["ErrorMessage"] = "Error interno. Contacte al soporte.";
            }

            return View(payment);
        }





        // GET: Mostrar formulario para editar un pago exitente (se ubica el pago con su id)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            return View(payment);
        }

        // POST: Actualizar un pago existente en la base de datos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,Amount,PaymentDate,Category,Reference,CreatedAt")] Payment payment)
        {
            try
            {
                if (id != payment.Id) return NotFound();

                if (ModelState.IsValid)
                {
                    _context.Update(payment);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Pago actualizado correctamente";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateConcurrencyException ex) // Regresar error especifico en caso de concurrencia 
            {
                if (!PaymentExists(payment.Id)) return NotFound();
                _logger.LogError(ex, $"Error de concurrencia en pago ID: {id}");
                TempData["ErrorMessage"] = "El pago fue modificado por otro usuario. Revise los cambios.";
            }
            catch (DbUpdateException ex) // Otro especifico en base de datos
            {
                _logger.LogError(ex, $"Error al actualizar pago ID: {id}");
                ModelState.AddModelError("", "No se pudo guardar. Por favor, intente nuevamente.");
            }
            catch (Exception ex) // Error inesperado, este error me haria volver a repasar el codigo, pero ya sabria que no fue bases de datos ni concurrencia
            {
                _logger.LogError(ex, $"Error inesperado al editar pago ID: {id}");
                TempData["ErrorMessage"] = "Error interno al procesar la solicitud";
            }

            return View(payment);
        }


        // POST: Elimina un pago especifico por id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var payment = await _context.Payments.FindAsync(id);
                if (payment == null)
                {
                    TempData["WarningMessage"] = "El pago no existe o ya fue eliminado";
                    return RedirectToAction(nameof(Index));
                }

                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Pago eliminado correctamente";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar pago ID: {id}");
                TempData["ErrorMessage"] = "Error al eliminar el pago";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST:Elimina varios pagos seleccionados 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSelected(int[] ids)
        {
            try
            {
                if (ids == null || ids.Length == 0)
                {
                    TempData["WarningMessage"] = "No se seleccionaron pagos para eliminar";
                    return RedirectToAction(nameof(Index));
                }

                var payments = await _context.Payments
                    .Where(p => ids.Contains(p.Id))
                    .ToListAsync();

                _context.Payments.RemoveRange(payments);
                await _context.SaveChangesAsync(); // ⚠️ Operación crítica

                TempData["SuccessMessage"] = $"Se eliminaron {payments.Count} pagos correctamente";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en eliminación masiva. IDs: {string.Join(",", ids)}");
                TempData["ErrorMessage"] = "Error al eliminar los pagos seleccionados";
            }

            return RedirectToAction(nameof(Index));
        }


        //Simple metodo para validar si el pago ya existe, lo separo para poderlo llamar rapidamente cuando necesite 
        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.Id == id);
        }
    }
}
