// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ClinicBooking.Web.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(SignInManager<IdentityUser> signInManager, ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            // Esegue il logout dell'utente corrente
            // SignOutAsync rimuove il cookie di autenticazione
            await _signInManager.SignOutAsync();
            // Log per debug (opzionale, utile per il corso)
            _logger.LogInformation("Utente Scollegato.");
            // Se è specificato un returnUrl valido, reindirizza lì
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // Altrimenti reindirizza alla home page
                // Questo comportamento è più user-friendly
                return RedirectToPage("/Index");
            }
        }
    }
}
