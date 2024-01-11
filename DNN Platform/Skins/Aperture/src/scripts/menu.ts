export const navFunctions = (() => {
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