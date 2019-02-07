using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using NeoXignaAPI;
using NeoXignaAPI.Entities;

namespace NeoXignaDemo.Controllers
{
    public class AppApiController : Controller
    {
        const string API_KEY = Global.API_KEY;
        const string SESSION_DOC_ID = "DocumentId";
        /// <summary>
        /// Mensaje mostrado al usuario en el app de NeoXigna cuando ingresa el PIN, permitiendo a un desarrollador externo
        /// mostrar al usuario información sobre lo que está por firmar
        /// </summary>
        const string END_USER_MESSAGE = "[Mensaje generado desde un app] Por favor firme el siguiente documento";
        const int EXPIRATION_SECONDS = 120; // 2 minutes
        const bool MANUAL_DESTROY = true; // Especifica si al subir un documento NeoXigna debe esperar una orden manual de destrucción. De no ser así NeoXigna destruye el documento en su tiempo por defecto (usualmente una hora)

        SignatureServices signatureServices = new SignatureServices(API_KEY);

#region Llamadas a NeoXigna para almacenamiento de documentos 

        public async Task<ActionResult> XML()
        {
            try
            {
                using (Stream xmlData = Assembly.GetExecutingAssembly().GetManifestResourceStream("Demo.xml"))
                {
                    Task<DocumentStoreResponse> documentTask = signatureServices.UploadXMLTAsync(fileData: xmlData,
                                                                                                contentLength: xmlData.Length,
                                                                                                endUserMessage: END_USER_MESSAGE,
                                                                                                generateQRHTMLImage: false,
                                                                                                awaitForManualDestroy: MANUAL_DESTROY,
                                                                                                signatureExpirationSecondstionSeconds: EXPIRATION_SECONDS);
                    DocumentStoreResponse document = await documentTask;

                    Session.Add(SESSION_DOC_ID, document.documentId);

                    return File(System.Text.Encoding.UTF8.GetBytes(document.qrText),
                                System.Net.Mime.MediaTypeNames.Text.Plain); ;
                }
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.InternalServerError, Global.HandleError(ex));
            }
        }

        public async Task<ActionResult> XMLXL()
        {
            try
            {
                using (Stream xmlData = Assembly.GetExecutingAssembly().GetManifestResourceStream("Demo.xml"))
                {
                    Task<DocumentStoreResponse> documentTask = signatureServices.UploadXMLXLAsync(fileData: xmlData,
                                                                                                contentLength: xmlData.Length,
                                                                                                endUserMessage: END_USER_MESSAGE,
                                                                                                generateQRHTMLImage: false,
                                                                                                awaitForManualDestroy: MANUAL_DESTROY,
                                                                                                signatureExpirationSecondstionSeconds: EXPIRATION_SECONDS);
                    DocumentStoreResponse document = await documentTask;

                    Session.Add(SESSION_DOC_ID, document.documentId);

                    return File(System.Text.Encoding.UTF8.GetBytes(document.qrText),
                                System.Net.Mime.MediaTypeNames.Text.Plain); ;
                }
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.InternalServerError, Global.HandleError(ex));
            }
        }
#endregion


#region Funciones HTTP de soporte
        /// <summary>
        /// Consulta local para verificar si el identificador de documento ya fue firmado.
        /// </summary>
        /// <returns>1: firmando | 0: no fue firmado| otros: resultado de firma, en este caso un string con infomación con el siguiente formato [nombre]|[identificación]|[fecha]</returns>
        [HttpGet]
        public ActionResult IsSigned()
        {
            string documentId = Session[SESSION_DOC_ID] != null ? Session[SESSION_DOC_ID].ToString() : "-";
            string result;
            if (NeoxignaCallbackController.SIGNED_DOCS_BY_ID.ContainsKey(documentId))
            {
                DocumentDownload download = NeoxignaCallbackController.SIGNED_DOCS_BY_ID[documentId];
                StringBuilder signatureInfo = new StringBuilder();
                foreach (SignatureVerification signature in download.signatureVerifications)
                {
                    if (signatureInfo.Length > 0)
                        signatureInfo.Append("\n");

                    // La hora necesita ser convertida a hora de CR, debido a que aca podemos recibir la hora en formado del huso horario del servidor
                    TimeZoneInfo targetTimeZone = TimeZoneInfo.CreateCustomTimeZone("CR", TimeSpan.FromHours(-6), "CR Time", "CR Time");
                    string crSignTime = signature.signDate != null ?
                                        " | " + TimeZoneInfo.ConvertTime(signature.signDate.Value, targetTimeZone).ToString() :
                                        string.Empty;

                    signatureInfo.Append(signature.signedBy + " | " + signature.signedBySerial + crSignTime);
                }
                signatureInfo.Append("\n");
                bool destroyed = NeoxignaCallbackController.DOC_DESTROYED_BY_ID.ContainsKey(documentId) && NeoxignaCallbackController.DOC_DESTROYED_BY_ID[documentId];
                signatureInfo.Append(destroyed ? "documento destruido en neoxigna" : "No se pudo destruir el documento en neoxigna");
                result = signatureInfo.ToString();
            }
            else if (NeoxignaCallbackController.SIGNING_DOCS_BY_ID.Contains(documentId))
            {
                result = "1";
            }
            else
            {
                result = "0";
            }

            return File(Encoding.UTF8.GetBytes(result),
                                System.Net.Mime.MediaTypeNames.Text.Plain);
        }

#endregion


    }
}
