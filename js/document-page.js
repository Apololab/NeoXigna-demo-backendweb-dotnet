var HomePage = function(){
    self = this;
    self.documentId = '';
   
   
    self.init = function(){
        self.documentId = $('#documentId').val();
        self.refresh();
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