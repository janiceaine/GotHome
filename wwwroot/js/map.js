let map; // keep global reference in module scope
let markers = []; // store all markers

export async function loadGoogleMaps(apiKey) {
	// dynamically load Maps JS
	const { AdvancedMarkerElement } = await import(
		`https://maps.googleapis.com/maps/api/js?key=${apiKey}&v=beta&libraries=maps,marker`
	);

	// initialize map
	map = new google.maps.Map(document.getElementById('map'), {
		center: { lat: 0, lng: 0 },
		zoom: 2,
	});

	// add current location marker
	if (navigator.geolocation) {
		navigator.geolocation.getCurrentPosition((position) => {
			const pos = {
				lat: position.coords.latitude,
				lng: position.coords.longitude,
			};
			map.setCenter(pos);
			map.setZoom(15);

			addMarker(pos.lat, pos.lng, 'blue', 'current location');
		});
	}

	return { map, AdvancedMarkerElement };
}

// add a marker dynamically later
export function addMarker(lat, lng, color = 'red', title = 'location') {
	if (!map) return;

	const marker = new google.maps.Marker({
		position: { lat, lng },
		map: map,
		content: `<div style="background:${color};width:20px;height:20px;border-radius:50%;border:2px solid white;"></div>`,
		title: title,
		label: {
			text: title,
			color: 'black',
			fontSize: '14px',
			fontWeight: 'bold',
		},
		icon: {
			url: `https://maps.google.com/mapfiles/ms/icons/${color}-dot.png`,
		},
	});

	markers.push(marker);
	return marker;
}
