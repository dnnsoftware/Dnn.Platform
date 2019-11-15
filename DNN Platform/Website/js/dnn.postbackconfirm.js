window.postBackConfirm = function(text, mozEvent, oWidth, oHeight, callerObj, oTitle) 
{
	try
	{
		var ev = mozEvent ? mozEvent : window.event; //Moz support requires passing the event argument manually 
		//Cancel the event 
		ev.cancelBubble = true; 
		ev.returnValue = false; 
		if (ev.stopPropagation) ev.stopPropagation(); 
		if (ev.preventDefault) ev.preventDefault(); 
	       
		//Determine who is the caller 
		var callerObj = ev.srcElement ? ev.srcElement : ev.target; 

		//Call the original radconfirm and pass it all necessary parameters 
		if (callerObj) 
		{ 
			//Show the confirm, then when it is closing, if returned value was true, automatically call the caller's click method again. 
			var callBackFn = function (arg) 
			{ 
				if (arg) 
				{ 
					callerObj["onclick"] = ""; 
					if (callerObj.click) callerObj.click(); //Works fine every time in IE, but does not work for links in Moz 
					else if (callerObj.tagName == "A") //We assume it is a link button! 
					{ 
						try 
						{ 
							eval(callerObj.href) 
						} 
						catch(e){} 
					} 
				} 
			} 
			
			if (oWidth == null || oWidth == "")
				oWidth = 350
			if (oHeight == null || oHeight == "")
				oHeight = 175
			if (oTitle == null || oTitle == "")
				oTitle = 'Confirm'
				
			radconfirm(text + "<br /><br />", callBackFn, oWidth, oHeight, callerObj, oTitle); 
		} 
		return false;
	}
	catch (ex)
	{
		return true;
	} 
};
