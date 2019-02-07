using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using NeoXignaAPI;
using NeoXignaAPI.Entities;

namespace NeoXignaDemo.Controllers
{
    public class NeoxignaCallbackController : Controller
    {
        private const int MAX_DOCS_IN_MEMORY = 10;

        SignatureServices signatureServices = new SignatureServices(Global.API_KEY);

        public static Dictionary<string, DocumentDownload> SIGNED_DOCS_BY_ID = new Dictionary<string, DocumentDownload>();
        public static Dictionary<string, bool> DOC_DESTROYED_BY_ID = new Dictionary<string, bool>();

        public static List<string> SIGNING_DOCS_BY_ID = new List<string>();
        private static List<string> SIGNED_DOC_IDS = new List<string>(); // Solamente para llevar el orden

        #region NeoXigna Callback

        /// <summary>
        /// En esta funcion se recibe el aviso de que el documento ya fue firmado y se puede proceder a descargar. Se debe notar que 
        /// esta función corresponde al URL del callback especificado en la subscripción (APIKEY) y para efectos de esta DEMO funciona debido a que 
        /// está servida en https://neoxignademo.azurewebsites.net/NeoxignaCallback/Index.
        /// </summary>
        /// <returns>The sign callback.</returns>
        /// <param name="documentId">Identificador del documento que ya fue firmado</param>
        [HttpPost]
        public async Task<ActionResult> Index(string documentId, string downloadKey)
        {
            System.Console.WriteLine("Received callback " + documentId);

            if (!SIGNED_DOCS_BY_ID.ContainsKey(documentId))
            {
                SIGNING_DOCS_BY_ID.Add(documentId);

                Task<DocumentDownload> documentTask = signatureServices.DownloadDocumentAsync(documentId, downloadKey); // Descarga el documento solo una vez
                SIGNED_DOCS_BY_ID[documentId] = await documentTask; // Descarga el documento

                bool destroyed = await DestroyDocumentAsync(documentId);
                DOC_DESTROYED_BY_ID.Add(documentId, destroyed); // Luego de descargado se destruye en NeoXigna

                SIGNED_DOC_IDS.Add(documentId);
                if (SIGNED_DOC_IDS.Count > MAX_DOCS_IN_MEMORY)
                {
                    string oldestDocId = SIGNED_DOC_IDS[0];
                    SIGNED_DOC_IDS.Remove(oldestDocId);
                    if (SIGNED_DOCS_BY_ID.ContainsKey(oldestDocId))
                        SIGNED_DOCS_BY_ID.Remove(oldestDocId);
                }
                SIGNING_DOCS_BY_ID.Remove(documentId);
            }
            return new EmptyResult();
        }

        #endregion


        #region Utilidades
        private async Task<bool> DestroyDocumentAsync(string documentId)
        {
            try
            {
                await signatureServices.DestroyDocumentDataAsync(documentId);
                Console.WriteLine("El documento fue destruido en neoxigna");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("No fue posible destruir el documento");
                Console.WriteLine(ex.StackTrace);
                return false;
            }
        }

        #endregion
    }
}
