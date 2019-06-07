# NeoXigna-demo-backendweb-dotnet

Este proyecto es un backend de demostración de la integración con NeoXigna, utilizando .NET MVC

Se compone de 3 partes principales (Controllers)
- AppApiController: Contiene endpoints HTTP que usan las apps móviles de demo para interactuar con la firma digital
- HomeController: Contiene las vistas web que demuestran cómo una página web puede interactuar con NeoXigna, ya sea firmando en escritorio, escaneando un código QR desde NeoXigna en móvil, o abriendo la página web propiamente en el móvil e implementando un botón que permite abrir NeoXigna
- NeoxignaCallbackController: Contiene únicamente el endpoint de Callback que utiliza NeoXigna backend para enviar el download key para descargar el documento firmado una vez preparado.

NOTA: Para que el Callback funcione, este proyecto debe estar publicado en un servidor accesible desde internet. Dicha dirección web pública debe ser notificada al equipo de NeoXigna para ligarla al API_KEY del desarrollador.Por tanto, si este proyecto se corre localmente el Callback <b>nunca será llamado</b> a excepción de que el ambiente de desarrollo local tenga un IP pública.

Para cambiar el API_KEY, se debe cambiar la variable Global.API_KEY, en el archivo Global.asax.cs

<br/>
<h1>Dependencias</h1>

Para las llamadas a NeoXigna backend se incluyen dos paquetes NuGet, dentro del directorio Dependencies/, para instalarlos seguir esta guía

https://hassantariqblog.wordpress.com/2016/12/04/asp-net-install-nuget-package-nupkg-file-locally/

O bien se puede agregar el siguiente repositorio de Nuget

http://repo.apololab.com/repository/apolo-nuget-public/

<br/>
<h1>Prueba en vivo</h1>

Este proyecto esta publicado en la siguiente dirección web 

http://neoxignademo.azurewebsites.net/