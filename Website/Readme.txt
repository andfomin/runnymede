----------------------------- PROJECT SETUP ---------------------------
--------------
Backup/Restore the Runnymede database. Add 'websitelogin' (pwd:123qweASD), recreate 'websiteuser' (with 'websitelogin', add it to 'websiterole') .
--------------
Add to C:\Windows\System32\drivers\etc\hosts
127.0.0.1   dev.englc.com 
--------------
Add the website to C:\Users\Andrey\Documents\IISExpress\config\applicationhost.config manually. Visual Studio can create automatically only 'localhost' sites.
<site name="Website" id="1">
    <application path="/" applicationPool="Clr4IntegratedAppPool">
        <virtualDirectory path="/" physicalPath="C:\Users\Andrey\Documents\Dignicom\Runnymede\Website" />
    </application>
    <bindings>
        <binding protocol="http" bindingInformation="*:80:dev.englc.com" />
        <binding protocol="https" bindingInformation="*:443:dev.englc.com" />
    </bindings>
</site>
--------------
netsh http show sslcert > c:\users\andrey\downloads\01.txt
Find the thumbprint for the general localhost cert created by IIS Express
netsh http add sslcert ipport=0.0.0.0:443 appid={214124cd-d05b-4309-9af9-9caa44b2b74a} certhash=YOURCERTHASHHERE
--------------
Run Visual Studio as Administrator. VS can start only localhost websites in IIS Express in the user mode.
--------------
--------------
-------------------------------- BOWER  ---------------------------------------
cd C:\Users\Andrey\Documents\Dignicom\Runnymede\Website
bower list
bower install <package>
bower-installer
rem +http://bower.io
rem +http://joshbranchaud.com/blog/2014/02/19/Managing-Single-Files-With-Bower.html
rem +https://github.com/blittle/bower-installer



