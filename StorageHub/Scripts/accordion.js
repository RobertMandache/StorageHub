(function($){
    $.fn.extend({

    //pass the options variable to the function
    accordion: function(options) {
        
		var defaults = {
			accordion: 'true',
			speed: 300,
			closedSign: '[+]',
			openedSign: '[-]',
			open: 'glyphicon-folder-open',
			close: 'glyphicon-folder-close'
		};

		// Extend our default options with those provided.
		var opts = $.extend(defaults, options);
		//Assign current element to variable, in this case is UL element
 		var $this = $(this);
 		
 		//add a mark [+] to a multilevel menu
 		$this.find("li").each(function() {
 			if($(this).find("ul").size() != 0){
 			    //add the multilevel sign next to the link
 			    '<span class="glyphicon glyphicon-folder-open"></span>'
                '<span class="glyphicon glyphicon-folder-close"></span>'
 			    
                $(this).find("a:first").append('<span class="glyphicon glyphicon-folder-close"></span>');
                
                
 				//avoid jumping to the top of the page when the href is an #
 				if($(this).find("a:first").attr('href') == "#"){
 		  			$(this).find("a:first").click(function(){return false;});
 		  		}
 			}
 			//$(this).children("a.qwe").append('<span class="glyphicon glyphicon-folder-close"></span>');
 			
 		});

 		//open active level
 		$this.find("li.active").each(function() {
 			$(this).parents("ul").slideDown(opts.speed);
 			$(this).parents("ul").parent("li").find("span:first").toggleClass(opts.close).toggleClass(opts.open);
 		});

 		$this.find("li a").not('.not-queried').click(function() {
  			if($(this).parent().find("ul").size() != 0){
  				if(opts.accordion){
  					//Do nothing when the list is open
  					if(!$(this).parent().find("ul").is(':visible')){
  						parents = $(this).parent().parents("ul");
  						visible = $this.find("ul:visible");
  						visible.each(function(visibleIndex){
  							var close = true;
  							parents.each(function(parentIndex){
  								if(parents[parentIndex] == visible[visibleIndex]){
  									close = false;
  									return false;
  								}
  							});
  							if(close){
  								if($(this).parent().find("ul") != visible[visibleIndex]){
  									$(visible[visibleIndex]).slideUp(opts.speed, function(){
  									    $(this).parent("li").find("span:first").toggleClass(opts.close).toggleClass(opts.open);
  									});
  									
  								}
  							}
  						});
  					}
  				}
  				if($(this).parent().find("ul:first").is(":visible")){
  					$(this).parent().find("ul:first").slideUp(opts.speed, function(){
  					    $(this).parent("li").find("span:first").delay(opts.speed).toggleClass(opts.close).toggleClass(opts.open);
  					});
  					
  					
  				}else{
  					$(this).parent().find("ul:first").slideDown(opts.speed, function(){
  					    $(this).parent("li").find("span:first").delay(opts.speed).toggleClass(opts.close).toggleClass(opts.open);
  					});
  				}
  			}
  		});
    }
});
})(jQuery);