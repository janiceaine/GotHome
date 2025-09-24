//stores the map element
let map;
//stores all map markers
let markers = [];

//store autocomplete
let autocomplete;

//watch id for cotinuous gelocation
let watchId = null;

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

	//setup autocomplete on text input
	const addressInput = document.querySelector('#home-address');
	if (addressInput) {
		autocomplete = new google.maps.places.Autocomplete(addressInput, {
			types: ['geocode'],
			componentRestrictions: { country: 'us' },
		});

		autocomplete.addListener('place_changed', function () {
			const nearPlace = autocomplete.getPlace();
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

	//notify map has been initialized
	window.dispatchEvent(new Event('mapInitialized'));
}

//make initMap globally accessible for Google Maps API callback; could not find function otherwise
window.initMap = initMap;

//get current location
// Get and track current location
export function getCurrentLocation(location_title = 'Current Location') {
	if (!navigator.geolocation) {
		console.error('Geolocation not supported');
		alert(
			'Your browser does not support geolocation. Showing default map view.'
		);
		map.setCenter({ lat: 37.7749, lng: -122.4194 }); // Default: San Francisco
		map.setZoom(10);
		return;
	}

	// Prompt user for permission
	if (
		!confirm('Allow us to track your location for 5 minutes to update the map?')
	) {
		alert('Location access skipped. Showing default map view.');
		map.setCenter({ lat: 37.7749, lng: -122.4194 });
		map.setZoom(10);
		return;
	}

	// Clear existing current location marker
	const currentLocationMarker = markers.find((m) => m.title === location_title);
	if (currentLocationMarker) {
		currentLocationMarker.map = null; // Remove from map
		markers = markers.filter((m) => m !== currentLocationMarker);
	}

	// Start watching position
	watchId = navigator.geolocation.watchPosition(
		(position) => {
			const pos = {
				lat: position.coords.latitude,
				lng: position.coords.longitude,
			};
			console.log(
				'Geolocation update, centering map at:',
				pos,
				'Accuracy:',
				position.coords.accuracy
			);
			if (position.coords.accuracy > 100) {
				console.warn('Low accuracy:', position.coords.accuracy, 'meters');
			}
			map.setCenter(pos);
			map.setZoom(15);
			addMarker(pos.lat, pos.lng, 'blue', location_title);
			// Dispatch locationUpdated event
			window.dispatchEvent(new CustomEvent('locationUpdated', { detail: pos }));
		},
		(error) => {
			console.error('Geolocation error:', error);
			if (error.code === error.PERMISSION_DENIED) {
				alert('Location access denied. Showing default map view.');
				map.setCenter({ lat: 37.7749, lng: -122.4194 });
				map.setZoom(10);
			} else {
				alert('Unable to retrieve location. Please try again.');
			}
		},
		{
			enableHighAccuracy: true,
			timeout: 10000,
			maximumAge: 0,
		}
	);

	// Stop tracking after 5 minutes
	setTimeout(() => {
		if (watchId !== null) {
			navigator.geolocation.clearWatch(watchId);
			watchId = null;
			console.log('Stopped tracking location after 5 minutes');
			alert('Location tracking stopped after 5 minutes.');
		}
	}, 5 * 60 * 1000); // 5 minutes
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
