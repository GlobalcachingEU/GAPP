var map;
var markers = [];
var curposMarker;
var wptmarkers = [];
var selectedMarker = "";
var ignoreCenter = false;
var custonOverlays = [];
var geocoder;
var activeInfoWindow = null;

//icons
//geocaches
//waypoints
//circels

function init() {
    var myOptions = {
        zoom: 13,
        center: new google.maps.LatLng(0.0, 0.0),
		scaleControl: true,
        mapTypeId: google.maps.MapTypeId.ROADMAP
    };
    map = new google.maps.Map(document.getElementById("map"), myOptions);
	geocoder = new google.maps.Geocoder();
    map.enableKeyDragZoom({
        visualEnabled: false,
        visualPosition: google.maps.ControlPosition.LEFT,
        visualPositionOffset: new google.maps.Size(35, 0),
        visualPositionIndex: null,
        visualSprite: "http://maps.gstatic.com/mapfiles/ftr/controls/dragzoom_btn.png",
        visualSize: new google.maps.Size(20, 20),
        visualTips: {
            off: "Turn on",
            on: "Turn off"
        }
    });
    curposMarker = new google.maps.Marker({ title: 'none', 'map': map, draggable: false, icon: curposIcon, position: map.getCenter(), flat: true, visible: false });

	//panToBounds

	updateGeocaches(gcList);
	updateWaypoints(wpList);
	updateCircels(circList);
}

function setMapCenter(lat, lon) {
		map.setCenter(new google.maps.LatLng(lat, lon));
}

function setCurrentPosition(valid, lat, lon) {
	if (valid) {
		curposMarker.setPosition(new google.maps.LatLng(lat, lon));
		curposMarker.setVisible(true);
	}
	else {
		curposMarker.setVisible(false);
	}
}

function updateCircels(cls) {
	for (var i = 0; i < cls.length; i++) {
		new google.maps.Circle({map: map, center: new google.maps.LatLng(cls[i].a, cls[i].b), radius: cls[i].c, fillOpacity: cls[i].d, strokeOpacity: cls[i].e, fillColor: cls[i].f, strokeColor: cls[i].f});
	}
}

function updateWaypoints(wps) {
	wptmarkers.length = 0;
	for (var i = 0; i < wps.length; i++) {
		wptmarkers.push(createWaypoint(wps[i].a,new google.maps.LatLng(wps[i].b, wps[i].c), wps[i].d, wps[i].e));
	}
}

function createWaypoint(id,point,ic,balloonCnt) {
   var marker = new google.maps.Marker({ 'title': id, map: map, draggable: false, icon: ic, position: point, flat: false, visible: true });
   var iw = new google.maps.InfoWindow();
   iw.setContent(balloonCnt);
   google.maps.event.addListener(marker, 'click', function () {
   if (activeInfoWindow!=null) activeInfoWindow.close();
   activeInfoWindow = iw;
   iw.open(map, marker);
   });
   return marker;
}

function updateGeocaches(wps) {
	markers.length = 0;
	for (var i = 0; i < wps.length; i++) {
		var marker = new google.maps.Marker({
			position: new google.maps.LatLng(wps[i].b, wps[i].c),
			icon: wps[i].d,
			title: wps[i].a,
			map: map
		});
		addClickListener(marker);
		markers.push(marker);
	}
}

function addClickListener(marker) {
	var gccode = marker.getTitle();
	google.maps.event.addListener(marker, 'click', function () {
		ignoreCenter = true;
		selectedMarker = gccode;
	});
}

function getSelectedGeocache() {
	var tmp = selectedMarker;
	selectedMarker = "";
	return tmp;
}

function setGeocache(cacheType, gccode, name, lat, lon, gcic) {
	if (ignoreCenter)
	{
		ignoreCenter = false;
	}
	else
	{
		map.setCenter(new google.maps.LatLng(lat, lon));
	}
}

function addPolygons(polys) {
    eval("var ps = " + polys);
    for (var i=0; i<ps.length; i++) {
        createArea(ps[i]);
    }
}

function zoomToBounds(minlat, minlon, maxlat, maxlon) {
    map.fitBounds(new google.maps.LatLngBounds(new google.maps.LatLng(minlat, minlon), new google.maps.LatLng(maxlat, maxlon)));
}

function createArea(area) {
    var points = [];
    for (var i = 0; i < area.points.length; i++) {
        points.push(new google.maps.LatLng(area.points[i].lat, area.points[i].lon));
    }
    custonOverlays.push(new google.maps.Polygon({ clickable: false, paths: points, map: map, fillOpacity: 0.1 }));
}

function setCenter(lat, lon) {
    map.setCenter(new google.maps.LatLng(lat, lon));
}


function onResize() {
  $('#map').width($(document).width()-40);
  $('#map').height($(document).height()-40);
}

$(window).resize(function() {
	onResize();
});

$(document).ready(function() {
    init();
	onResize();
});