var HomePage = function(){
    self = this;
    self.documentId = '';
    self.beginDate = new Date();
    self.documentRemainingSeconds;
   
   
    self.init = function(){
        self.documentId = $('#documentId').val();
        self.documentRemainingSeconds = $('#remainingSeconds').val();
        self.updateRemainingSeconds();
        self.refresh();
    }
    
    self.updateRemainingSeconds = function(){
        var seconds = Math.round((new Date().getTime() - self.beginDate.getTime()) / 1000);
        var remainingSeconds = self.documentRemainingSeconds - seconds;
        if (remainingSeconds <= 0){
            window.location.replace( GlobalPreprocessed.RootPath+'Home/');
        } else {
            var secondsTxt = remainingSeconds > 1 ? 'segundos' : 'segundo';
            $('#txt-expiration-seconds').text( remainingSeconds+' '+secondsTxt);
            setTimeout(self.updateRemainingSeconds, 1000);
        }
    }
    self.refresh = function () {
    
        console.log(self.documentId);
    
        $.ajax({
            url: GlobalPreprocessed.RootPath+'Home/IsSigned?documentId='+self.documentId,
            type: "GET",
            complete: function (result) {
                if (result.status == 200) {
                    var isSigned = result.responseJSON == '1';
                    if (isSigned){
                        window.location.replace( GlobalPreprocessed.RootPath+'Home/SignReady?documentId='+self.documentId );
                    }
                } 
                // Schedule the next request when the current one's complete
                setTimeout(self.refresh, 2000);
            }
        });
    }
    

}

var Page = new HomePage();