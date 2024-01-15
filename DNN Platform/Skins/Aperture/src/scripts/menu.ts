export const navFunctions = (() => {
	// add event listeners to all anchor tags with a hash in href
	const anchorsWithHash: NodeListOf<HTMLAnchorElement> = document.querySelectorAll('a[href^="/#"]');
	anchorsWithHash.forEach(anchorWithHash => {
		// listen for click event
		anchorWithHash.addEventListener('click', event => {
			// check if anchor is on the same page
			if (location.pathname.replace(/^\//, '') === anchorWithHash.pathname.replace(/^\//, '') && location.hostname === anchorWithHash.hostname) {
				// prevent default anchor click behavior
				event.preventDefault();
				// figure where to scroll to
				const target: HTMLAnchorElement | null = document.querySelector(anchorWithHash.hash);
				// check if target exists
				if (target !== null) {
					// get target position
					const targetTop = target.getBoundingClientRect().top;
					// get window position
					const windowTop = window.scrollY;
					// animate scroll to anchor with id
					window.scrollTo({
						top: targetTop + windowTop - 150,
						behavior: 'smooth'
					});
				}
			};
		});
	});

	// add event listeners to expands
	const navExpand = document.querySelectorAll('.nav-expand');
  
	navExpand.forEach(item => {
	  item.querySelector('.nav-expand-link')?.addEventListener('click', () => item.classList.add('active'));
	  item.querySelector('.nav-back-link')?.addEventListener('click', () => item.classList.remove('active'));
	});
  
	// setup and add overlay
	let overlay = document.createElement('div');
	overlay.setAttribute('id', 'body-overlay');
	document.body.appendChild(overlay).classList.add('aperture-d-none');
  
	// target hamburger menu
	const responsiveMenu = document.getElementById('nav-mobile');
	const bodyOverlay = document.getElementById('body-overlay');
  
	responsiveMenu?.addEventListener('click', function() {
	  // toggle menu
	  document.body.classList.toggle('nav-is-toggled');
	  bodyOverlay?.classList.toggle('aperture-d-none');
	});
  
	// if overlay is clicked
	bodyOverlay?.addEventListener('click', function() {
	  // toggle menu and overlay
	  document.body.classList.toggle('nav-is-toggled');
	  bodyOverlay?.classList.toggle('aperture-d-none');
	});
  });