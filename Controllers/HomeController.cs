using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using NeoXignaAPI;
using NeoXignaAPI.Entities;
using System.Threading.Tasks;

namespace NeoXignaDemo.Controllers
{
    public class HomeController : Controller
    {
        const string API_KEY = Global.API_KEY;
        const string END_USER_MESSAGE = "[Mensaje generado desde el cliente en web] Por favor firme el siguiente documento";
        const int EXPIRATION_SECONDS = 120; // 2 minutes
        const bool MANUAL_DESTROY = true; // Especifica si al subir un documento NeoXigna debe esperar una orden manual de destrucción. De no ser así NeoXigna destruye el documento en su tiempo por defecto (usualmente una hora)

        SignatureServices signatureServices = new SignatureServices(API_KEY);

#region Vistas y llamadas a NeoXigna 

        public ActionResult Index()
        {
            Apololab.Common.Http.ApoloHttpClient.DebugDelegate = (message) => Console.WriteLine(message);
            return View();
        }
        /// <summary>
        /// Sube el documento PDF al Engine de NeoXigna y obtiene el identificador de documento
        /// </summary>
        /// <returns>Vista inicial</returns>
        public async Task<ActionResult> PDF()
        {
            try {
                using (Stream pdfData = Assembly.GetExecutingAssembly().GetManifestResourceStream("Demo.pdf"))
                {
                    // No se solicita la imagen del QR si se consulta desde un móvil, por que en vez del QR mostramos un botón
                    bool generateQRImage = !IsMobile(Request);
#if MONO
                    DocumentStoreResponse document = signatureServices.UploadPDFAsync(fileData: pdfData,
                                                                                                contentLength: pdfData.Length,
                                                                                                endUserMessage: END_USER_MESSAGE,
                                                                                                generateQRHTMLImage: generateQRImage,
                                                                                                awaitForManualDestroy : MANUAL_DESTROY,
                                                                                                signatureExpirationSecondstionSeconds: EXPIRATION_SECONDS).Result;
#else
                    DocumentStoreResponse document = await signatureServices.UploadPDFAsync(fileData: pdfData,
                                                                                                contentLength: pdfData.Length,
                                                                                                endUserMessage: END_USER_MESSAGE,
                                                                                                generateQRHTMLImage: generateQRImage,
                                                                                                awaitForManualDestroy : MANUAL_DESTROY,
                                                                                                signatureExpirationSecondstionSeconds: EXPIRATION_SECONDS);
#endif
                    return View(document);
                }
            } 
            catch (Exception ex)
            {
                ViewData["error"] = Global.HandleError(ex);
                return View("Error");

            }
        }

        public async Task<ActionResult> XADEST()
        {
            try
            {
                using (Stream xmlData = Assembly.GetExecutingAssembly().GetManifestResourceStream("Demo.xml"))
                {
                    // No se solicita la imagen del QR si se consulta desde un móvil, por que en vez del QR mostramos un botón
                    bool generateQRImage = !IsMobile(Request);
#if MONO
                    DocumentStoreResponse document = signatureServices.UploadXMLTAsync(fileData: xmlData,
                                                                                                contentLength: xmlData.Length,
                                                                                                endUserMessage: END_USER_MESSAGE,
                                                                                                generateQRHTMLImage: generateQRImage,
                                                                                                awaitForManualDestroy: MANUAL_DESTROY,
                                                                                                signatureExpirationSecondstionSeconds: EXPIRATION_SECONDS).Result;
#else
                    DocumentStoreResponse document = await signatureServices.UploadXMLTAsync(fileData: xmlData,
                                                                                                contentLength: xmlData.Length,
                                                                                                endUserMessage: END_USER_MESSAGE,
                                                                                                generateQRHTMLImage: generateQRImage,
                                                                                                awaitForManualDestroy : MANUAL_DESTROY,
                                                                                                signatureExpirationSecondstionSeconds: EXPIRATION_SECONDS);
#endif

                    return View("XML",document);
                }
            }
            catch (Exception ex)
            {
                ViewData["error"] = Global.HandleError(ex);
                return View("Error");

            }
        }

        public async Task<ActionResult> XADESXL()
        {
            try
            {
                using (Stream xmlData = Assembly.GetExecutingAssembly().GetManifestResourceStream("Demo.xml"))
                {
                    // No se solicita la imagen del QR si se consulta desde un móvil, por que en vez del QR mostramos un botón
                    bool generateQRImage = !IsMobile(Request);
#if MONO
                    DocumentStoreResponse document = signatureServices.UploadXMLXLAsync(fileData: xmlData,
                                                                                                contentLength: xmlData.Length,
                                                                                                endUserMessage: END_USER_MESSAGE,
                                                                                                generateQRHTMLImage: generateQRImage,
                                                                                                awaitForManualDestroy: MANUAL_DESTROY,
                                                                                                signatureExpirationSecondstionSeconds: EXPIRATION_SECONDS).Result;
#else
                    DocumentStoreResponse document = await signatureServices.UploadXMLTAsync(fileData: xmlData,
                                                                                                contentLength: xmlData.Length,
                                                                                                endUserMessage: END_USER_MESSAGE,
                                                                                                generateQRHTMLImage: generateQRImage,
                                                                                                awaitForManualDestroy : MANUAL_DESTROY,
                                                                                                signatureExpirationSecondstionSeconds: EXPIRATION_SECONDS);
#endif

                    return View("XML", document);
                }
            }
            catch (Exception ex)
            {
                ViewData["error"] = Global.HandleError(ex);
                return View("Error");

            }
        }

        /// <summary>
        /// Vista a la que dirigimos el usuario cuando el documento ya fue firmado para mostrarle la información sobre la firma
        /// </summary>
        /// <returns>Vista que muestra al usuario la posibilidad de descargar el documento firmado</returns>
        /// <param name="documentId">Document identifier.</param>
        [HttpGet]
        public ActionResult SignReady(string documentId)
        {
            try
            {
                if (NeoxignaCallbackController.SIGNED_DOCS_BY_ID.ContainsKey(documentId))
                {

                    ViewData["document"] = NeoxignaCallbackController.SIGNED_DOCS_BY_ID[documentId];
                    ViewData["documentId"] = documentId;
                    ViewData["destroyed"] = NeoxignaCallbackController.DOC_DESTROYED_BY_ID.ContainsKey(documentId) && NeoxignaCallbackController.DOC_DESTROYED_BY_ID[documentId];

                    return View();
                } else
                {
                    ViewData["error"] = "El documento ha expirado o aún no se ha firmado";
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                ViewData["error"] = Global.HandleError(ex);
                return View("Error");
            }
        }

#endregion

#region Funciones HTTP de soporte
        /// <summary>
        /// Consulta local para verificar si el identificador de documento ya fue firmado. 
        /// Esta función es llamada periodicamente por javascript
        /// </summary>
        /// <returns>1 si el documento ya fue firmado, 0 si no</returns>
        /// <param name="documentId">Document identifier.</param>
        [HttpGet]
        public ActionResult IsSigned(string documentId)
        {
            char docSigned = NeoxignaCallbackController.SIGNED_DOCS_BY_ID.ContainsKey(documentId) ? '1' : '0';
            return new JsonResult() { Data = docSigned, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public ActionResult DownloadSigned(string documentId)
        {
            if (NeoxignaCallbackController.SIGNED_DOCS_BY_ID.ContainsKey(documentId))
            {
                var document = NeoxignaCallbackController.SIGNED_DOCS_BY_ID[documentId];
                byte[] fileBytes = document.fileData;
                string fileName = document.fileName;
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            else
            {
                return new EmptyResult();
            }

        }

#endregion

#region Utilidades


        public static string GetRootPath(HttpRequestBase request,UrlHelper urlHelper)
        {
            return (string.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Authority, urlHelper.Content("~")) + "/");
        }

        public static bool IsMobile(HttpRequestBase request)
        {
            return request.UserAgent.ToLower().Contains("android") ||
                          request.UserAgent.ToLower().Contains("iphone");
        }

#endregion
    }
}
