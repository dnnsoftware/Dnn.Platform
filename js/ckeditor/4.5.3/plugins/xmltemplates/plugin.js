/*
Copyright (c) 2003-2011, CKSource - Frederico Knabben. All rights reserved.
For licensing, see LICENSE.html or http://ckeditor.com/license
*/

(function()
{
	CKEDITOR.plugins.add( 'xmltemplates',
		{
			requires : [ 'dialog', 'templates', 'ajax', 'xml' ]
		});

	var templates = {},
		loadedTemplatesFiles = {};

	CKEDITOR.addTemplates = function( name, definition )
	{
		templates[ name ] = definition;
	};

	CKEDITOR.getTemplates = function( name )
	{
		return templates[ name ];
	};

	function loadXmlTemplates( urls, callback )
	{
		function loadedTemplate( xml )
		{
			// Parse the template and add it
			if ( !xml )
			{
				alert( 'XML template file not parsed' );
				return;
			}

			var templatesNode = xml.selectSingleNode( '/Templates' );

			if ( !templatesNode )
			{
				alert( 'The <Templates> node was not found' );
 				return ;
			}

			// Get the imagesBasePath attribute value.
			var imagesBasePath = templatesNode.getAttribute( 'imagesBasePath' ),
				templateName = templatesNode.getAttribute( 'templateName' ) || 'default',
				newTemplate = {},
				// Retrieve all <Template> nodes.
	 			templateNodes = xml.selectNodes( 'Template', templatesNode ),
				templates = newTemplate.templates = [];

			if ( imagesBasePath )
				newTemplate.imagesPath = CKEDITOR.getUrl( imagesBasePath );

			for ( var i = 0 ; i < templateNodes.length ; i++ )
 			{
 				var templateNode = templateNodes[ i ],
					template = {},
					title = templateNode.getAttribute( 'title' ) ,
 					image = templateNode.getAttribute( 'image' ) ,
 					description = xml.getInnerXml( 'Description', templateNode ) ,
					oPart = xml.selectSingleNode( 'Html', templateNode );

				title && ( template.title = title );
				image && ( template.image = image );
				description && ( template.description = description );
				template.html = oPart.text ? oPart.text : oPart.textContent;

				templates.push( template );
			}

			CKEDITOR.addTemplates( templateName, newTemplate );

			// Finish the loading
			if ( ++loadedCount == urls.length)
				setTimeout( callback, 0 );
		}

		var loadedCount = 0;

		for(var i=0; i<urls.length; i++)
			CKEDITOR.ajax.loadXml( urls[i].substring(4), loadedTemplate );
	}

	CKEDITOR.loadTemplates = function( templateFiles, callback )
	{
		function jsLoaded()
		{
			jsToLoad = [];
			if (xmlToLoad.length==0)
				setTimeout( callback, 0 );
		}
		function xmlLoaded()
		{
			xmlToLoad = [];
			if (jsToLoad.length==0)
				setTimeout( callback, 0 );
		}

		// Holds the templates files to be loaded.
		var jsToLoad = [],
			xmlToLoad = [],
			pending;

		if ( typeof templateFiles == 'string')
			templateFiles = [ templateFiles ];

		// Look for pending template files to get loaded.
		for ( var i = 0, count = templateFiles.length ; i < count ; i++ )
		{
			var template = templateFiles[ i ];
			if ( !loadedTemplatesFiles[ template ] )
			{
				( template.substring(0, 4) == "xml:" ? xmlToLoad : jsToLoad ).push( template );
				loadedTemplatesFiles[ template ] = 1;
			}
		}

		if ( jsToLoad.length )
		{
			pending = true;
			CKEDITOR.scriptLoader.load( jsToLoad, jsLoaded );
		}

		if ( xmlToLoad.length )
		{
			pending = true;
			loadXmlTemplates( xmlToLoad, xmlLoaded );
		}

		if ( !pending )
			setTimeout( callback, 0 );
	};
})();
