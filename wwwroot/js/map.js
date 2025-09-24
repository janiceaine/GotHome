//stores the map element
let map;
//stores all map markers
let markers = [];

//store autocomplete
let autocomplete;

//watch id for cotinuous gelocation
let watchId = null;
let currentLocationTrackingMarker = null;

//close threshold for presence detection, in meters
const REASONABLE_DISTANCE = 100;

//initialize the map
function initMap() {
	console.log('initMap called');

	const apiKey = window.mapConfig?.apiKey ?? '';
	const mapId = window.mapConfig?.mapId ?? '';
	console.log('API Key in map.js:', apiKey);
	console.log('Map ID in map.js:', mapId);

	//initiize a world map
	map = new google.maps.Map(document.getElementById('map'), {
		center: { lat: 0, lng: 0 },
		zoom: 2,
		mapId: mapId,
	});

	//setup autocomplete on home text input
	let autocomplete_home;
	const homeAddressInput = document.querySelector('#home-address');
	if (homeAddressInput) {
		autocomplete_home = new google.maps.places.Autocomplete(homeAddressInput, {
			types: ['geocode'],
			componentRestrictions: { country: 'us' },
		});

		autocomplete_home.addListener('place_changed', function () {
			const nearPlace = autocomplete_home.getPlace();
			console.log('Selected place:', nearPlace);

			if (nearPlace.geometry && nearPlace.geometry.location) {
				const lat = nearPlace.geometry.location.lat();
				const lng = nearPlace.geometry.location.lng();
				console.log('Lat:', lat, 'Lng:', lng);

				map.setCenter({ lat, lng });
				map.setZoom(14);

				addMarker(lat, lng, 'green', 'Home location');
			}
		});
	} else {
		console.error('#home-address input not found');
	}

	//start tracking current location
	getCurrentLocation();
	//setup autocomplete on event text input
	let autocomplete_event;
	const eventAddressInput = document.querySelector('#event-address');
	if (eventAddressInput) {
		autocomplete_event = new google.maps.places.Autocomplete(
			eventAddressInput,
			{
				types: ['geocode'],
				componentRestrictions: { country: 'us' },
			}
		);

		autocomplete_event.addListener('place_changed', function () {
			const nearPlace = autocomplete_event.getPlace();
			console.log('Selected place:', nearPlace);

			if (nearPlace.geometry && nearPlace.geometry.location) {
				const lat = nearPlace.geometry.location.lat();
				const lng = nearPlace.geometry.location.lng();
				console.log('Lat:', lat, 'Lng:', lng);

				map.setCenter({ lat, lng });
				map.setZoom(14);

				addMarker(lat, lng, 'purple', 'Event location');
			}
		});
	} else {
		console.error('#event-address input not found');
	}

	//notify map has been initialized
	window.dispatchEvent(new Event('mapInitialized'));
}

//make initMap and StopTrackingNow globally accessible; views could not find these function otherwise
window.initMap = initMap;
window.stopTrackingNow = stopTrackingNow;

// Get and track current location
export function getCurrentLocation(
	location_title = 'Current Location',
	durationMinutes = 1
) {
	if (!navigator.geolocation) {
		console.error('Geolocation not supported');
		alert('Your web browser does not support geolocation.');
		//AT&T Stadium
		map.setCenter({ lat: 32.747519, lng: -97.092994 });
		map.setZoom(10);
		return;
	}

	if (
		!confirm(
			`Allow GotHome to track your location for ${durationMinutes} minute${
				durationMinutes === 1 ? '' : 's'
			} to update the map?`
		)
	) {
		//hide tracking components
		document.getElementById('stop-tracking').classList.add('d-none');
		alert('Location tracking access has been denied by the user.');
		//AT&T Stadium
		map.setCenter({ lat: 32.747519, lng: -97.092994 });
		map.setZoom(10);
		return;
	}

	// Clear existing current location marker
	const currentLocationMarker = markers.find((m) => m.title === location_title);
	if (currentLocationMarker) {
		currentLocationMarker.map = null;
		markers = markers.filter((m) => m !== currentLocationMarker);
	}

	watchId = navigator.geolocation.watchPosition(
		(position) => {
			// Clear previous marker
			const prevMarker = markers.find((m) => m.title === location_title);
			if (prevMarker) {
				prevMarker.map = null;
				markers = markers.filter((m) => m !== prevMarker);
			}
			const pos = {
				lat: position.coords.latitude,
				lng: position.coords.longitude,
			};
			console.log('Geolocation update:', {
				pos,
				accuracy: position.coords.accuracy,
				timestamp: new Date().toISOString(),
			});
			if (position.coords.accuracy > 100) {
				console.warn('Low accuracy:', position.coords.accuracy, 'meters');
			}
			map.setCenter(pos);
			map.setZoom(15);
			currentLocationTrackingMarker = addMarker(
				pos.lat,
				pos.lng,
				'blue',
				location_title
			);
			window.dispatchEvent(new CustomEvent('locationUpdated', { detail: pos }));
		},
		(error) => {
			console.error('Geolocation error:', {
				code: error.code,
				message: error.message,
				timestamp: new Date().toISOString(),
			});
			if (error.code === error.PERMISSION_DENIED) {
				alert(
					'Location tracking denied. On Android, go to Settings > Apps > Chrome/Firefox > Permissions > Location and set to "Allow all the time".'
				);
				// Fallback to getCurrentPosition
				console.log('Falling back to getCurrentPosition');
				navigator.geolocation.getCurrentPosition(
					(position) => {
						const pos = {
							lat: position.coords.latitude,
							lng: position.coords.longitude,
						};
						console.log('getCurrentPosition success:', pos);
						map.setCenter(pos);
						map.setZoom(15);
						currentLocationTrackingMarker = addMarker(
							pos.lat,
							pos.lng,
							'blue',
							location_title
						);
					},
					(err) => {
						console.error('getCurrentPosition error:', err);
						alert('Location access failed: ' + err.message);
						map.setCenter({ lat: 37.7749, lng: -122.4194 });
						map.setZoom(10);
					},
					{ enableHighAccuracy: true, timeout: 10000, maximumAge: 0 }
				);
			} else {
				alert('Unable to retrieve location: ' + error.message);
				map.setCenter({ lat: 37.7749, lng: -122.4194 });
				map.setZoom(10);
			}
		},
		{
			enableHighAccuracy: true,
			timeout: 10000,
			maximumAge: 0,
		}
	);

	//not using in v1: update the user regarding the time remainining
	// let timeLeft = durationMinutes * 60;
	// const timer = setInterval(() => {
	// 	timeLeft--;
	// 	if (timeLeft <= 0 || watchId === null) {
	// 		clearInterval(timer);
	// 		document.getElementById('tracking-status').textContent =
	// 			'Tracking stopped';
	// 		//hide tracking components
	// 		document.getElementById('stop-tracking').classList.add('d-none');
	// 	} else {
	// 		document.getElementById(
	// 			'tracking-status'
	// 		).textContent = `Tracking for ${Math.floor(timeLeft / 60)}:${(
	// 			timeLeft % 60
	// 		)
	// 			.toString()
	// 			.padStart(2, '0')} remaining`;
	// 	}
	// }, 1000);

	// Stop tracking after specified duration
	setTimeout(() => {
		if (currentLocationTrackingMarker) {
			currentLocationTrackingMarker.map = null;
			markers = markers.filter((m) => m !== currentLocationTrackingMarker);
			currentLocationTrackingMarker = null;
		}
		if (watchId !== null) {
			navigator.geolocation.clearWatch(watchId);
			watchId = null;
			console.log(`Stopped tracking location`);
			alert(`GotHome location tracking has ended.`);
			document.getElementById('stop-tracking').classList.add('d-none');
		}
	}, durationMinutes * 60 * 1000);
}

//allow the user to end the tracking on demand
function stopTrackingNow() {
	if (confirm(`Stop GotHome location tracking now?`)) {
		watchId = null;
		if (currentLocationTrackingMarker) {
			currentLocationTrackingMarker.map = null;
			markers = markers.filter((m) => m !== currentLocationTrackingMarker);
			currentLocationTrackingMarker = null;
		}
		if (watchId !== null) {
			navigator.geolocation.clearWatch(watchId);

			console.log('Location tracking stopped manually and marker cleared');
			alert('Location tracking stopped and marker cleared.');
		} else {
			console.log('No active location tracking to stop');
			alert('No location tracking is active.');
			document.getElementById('stop-tracking').classList.add('d-none');
		}
	} else {
		alert('GotHome Location tracking access is still enabled.');
		return;
	}
}
// Add a marker using AdvancedMarkerElement
export function addMarker(lat, lng, color = 'red', title = 'location') {
	if (!map) {
		console.error('Map not initialized');
		return;
	}

	// Validate lat/lng
	if (
		typeof lat !== 'number' ||
		typeof lng !== 'number' ||
		isNaN(lat) ||
		isNaN(lng)
	) {
		console.error('Invalid coordinates:', { lat, lng });
		return;
	}

	console.log('Adding marker:', { lat, lng, color, title });

	// Create a PinElement for the marker's appearance
	const pin = new google.maps.marker.PinElement({
		background: color,
		borderColor: 'white',
		glyphColor: 'white',
		scale: 1.0,
	});

	// Create a DOM element for the label
	const labelElement = document.createElement('div');
	labelElement.style.color = 'black';
	labelElement.style.fontSize = '14px';
	labelElement.style.fontWeight = 'bold';
	labelElement.style.position = 'absolute';
	labelElement.style.transform = 'translate(0, 20px)'; // Position below marker
	labelElement.textContent = title;

	// Create the AdvancedMarkerElement
	const marker = new google.maps.marker.AdvancedMarkerElement({
		map: map,
		position: { lat, lng },
		content: pin.element,
		title: title,
	});

	// Append the label to the marker
	marker.element.appendChild(labelElement);

	markers.push(marker);
	return marker;
}
function updateVisibility() {
	const clearMarkerButton = document.querySelector('#clear-marker');
	const stopTrackingButton = document.querySelector('#stop-tracking');
	const trackingStatus = document.querySelector('#tracking-status');

	if (clearMarkerButton) {
		clearMarkerButton.classList.toggle('d-none', !currentLocationMarker);
	} else {
		console.warn('Element #clear-marker not found in DOM');
	}

	if (stopTrackingButton) {
		stopTrackingButton.classList.toggle('d-none', watchId === null);
	} else {
		console.warn('Element #stop-tracking not found in DOM');
	}

	if (trackingStatus) {
		trackingStatus.classList.toggle('d-none', watchId === null);
	} else {
		console.warn('Element #tracking-status not found in DOM');
	}
}
