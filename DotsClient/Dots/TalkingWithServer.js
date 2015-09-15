function TimerCount() {
    var timers = $(".time-left");

    for (var i = 0; i < timers.length; i++) {
        var time = parseInt($(timers[i]).text());
        time--;
        $(timers[i]).text(time);
    }
    setTimeout(TimerCount, 1000);
}

function timeOut() {
    setTimeout(TimerCount, 1000);
}

timeOut();
window.userPPInfo = "";
window.userTables = [];
window.hasCloseBox = false;
$(function () {
    // Create the connection to our SignalR-powered Chat Hub on the server.
    $.connection.hub.url = "http://boyanzhelyazkov.com/signalr";
    var signalRChatHub = $.connection.chatHub;

    // Start the SignalR Hub.
    $.connection.hub.start();

    $("a[href='#schedule']").click(function () {
        signalRChatHub.server.getTurnament("0", device.uuid);
    });

    $("a[href='#sitAndGo']").click(function () {
        signalRChatHub.server.getTurnament("1", device.uuid);

    });

    $("a[href='#free']").click(function () {
        signalRChatHub.server.getTurnament("2", device.uuid);
    });

    $(document).on("click", '.accept', function () {
        var invitedUserId = $("#inviteUserDiv").attr("inviteMeUser");
        var myId = $("#inviteUserDiv").attr("myId");
        var rows = $("#inviteUserDiv").attr("rows");
        var cols = $("#inviteUserDiv").attr("cols");
        var amount = $("#inviteUserDiv").attr("amount");

        signalRChatHub.server.acceptUserInvite(myId, invitedUserId, rows, cols, amount, - 1);
    });

    $('#login').click(function () {
        // Call Server method.
        $.connection.hub.qs = { deviceId: device.uuid };
        var username = $('#loginform').find('#username').val();
        var password = $('#loginform').find('#password').val();
        $("#auth-page").hide();
        if (userWebClientId == undefined) {
            debugger;
            $.connection.hub.start();
        }
        signalRChatHub.server.loginUserToLobby(device.uuid, username, password, userWebClientId);
        
    });

    $('#signin').click(function () {
        $('#registrationform').hide();
        $('#loginform').show();
    });

    $('#signup').click(function () {
        $('#registrationform').show();
        $('#loginform').hide();
    });

    $('#register').click(function () {
        // Call Server method.
        $.connection.hub.qs = { deviceId: device.uuid };
        var username = $('#registrationform').find('#username').val();
        var password = $('#registrationform').find('#password').val();
        var email = $('#registrationform').find('#email').val();
        if (userWebClientId == undefined) {
            $.connection.hub.start();
        }
        signalRChatHub.server.registerUser(device.uuid, username, password, userWebClientId, email);
        $("#registrationform").val($('#username').val());
        $('#registrationform').find('#username').val("");
        $('#registrationform').find('#password').val("");
        $(".tab-content").show();
        $(".nav-tabs").show();
        $("#auth-page").hide();
    });

    signalRChatHub.client.broadcastUsers = function (name, message) {
        var className = 'u' + name + '-' + message;
        var myDeviceId = device.uuid;
        
        if ($('.' + className).length == 0) {
            $("#lobbyUsers").find(".className").remove();
            $('#lobbyUsers').append('<div class="user-holder"><div class="col col-xs-12 row-holder" > <div class="col col-xs-6">' + message + '</div>' +
                            '<div class="col col-xs-6"><input class="input-invite" class="amount '+name+'"  placeholder="enter amount" type="number" /></div></div>' +
                            '<div class="col col-xs-12"><p class="invite btn btn-success" style="" user-id="' + name + '">Invite</p></div></div>');
        }
    };

    signalRChatHub.client.clearCurrentTurnamentsTab = function (tabToClear) {
        //TODO Clear Tab
        $("#"+tabToClear).html("");
    };

    signalRChatHub.client.showTurnament = function (turnament, tabToClear, registered) {

    
        //TODO Show Turnaments
        var innerHtml = "<div class='turnament' turnamentId = '" + turnament.Id + "'>" + "<p class='name'>" + turnament.Name+"</p>";
        
         
        if (turnament.isSeatAndGo == 1) {
            turnament.StartTime = turnament.StartTime.split('T')[0];
            innerHtml += "<p class='time'>"+turnament.StartTime+" </p>";
        }


        innerHtml += "<p class='colTur colTurLeft'>"
        innerHtml += "<span class='cost'> $" + turnament.EntryCost + "</span>+";
        innerHtml += "<span class='taxes'>" + turnament.Taxes + "</span>" ;

        innerHtml += "<p class='colTur glyphicon glyphicon-user'>";
        innerHtml += "<span class='taken'>" + turnament.TakenSeats + "</span>/";
        innerHtml += "<span class='seats'>" + turnament.Seats + "</span>";
        innerHtml += "</p>";
        // if (turnament.isSeatAndGo == true) {
            innerHtml += "<button type='button' class='reg btn btn-success'";
            if (registered == true) {
                innerHtml += 'style="display:none"';
            }
            innerHtml += ">Register</button>";
     
            innerHtml += "<button type='button' class='unreg btn btn-danger'";
            if (registered == false) {
                innerHtml += 'style="display:none"';
            }
            innerHtml +=  ">UnRegister</button>";
        
        // }

        innerHtml += "<div class='clear'></div>";
        innerHtml += "</div>";

        
        $("#"+tabToClear).append(innerHtml);
    };

    signalRChatHub.client.alertTurnament = function (turnament) {
        alert(turnament.Id + " Name: " + turnament.Name + " Seats: " + turnament.Seats + " SitAndGo:" + turnament.IsSeatAndGo);
    };


    signalRChatHub.client.registrationStatus = function (turnamentId, status) {
        if (status == true) {
            $(".turnament[turnamentId='" + turnamentId + "'").find(".unreg").show();
            $(".turnament[turnamentId='" + turnamentId + "'").find(".reg").hide();
        }
        else if (status == false) {
            $(".turnament[turnamentId='" + turnamentId + "'").find(".reg").show();
            $(".turnament[turnamentId='" + turnamentId + "'").find(".unreg").hide();
        } 
    };

    signalRChatHub.client.turnamentStatus = function (turnamentId, freeSeats) {
        var seats = $(".turnament[turnamentId='" + turnamentId + "'").find(".seats").text();
        if (seats == freeSeats) {
            $(".turnament[turnamentId='" + turnamentId + "'").hide();
        } 
        $(".turnament[turnamentId='" + turnamentId + "'").find(".taken").html(freeSeats);
    };


    $(document).on("click", '.reg', function () {
        // Call Server method.
        var myId = device.uuid;
        var turnamentId = $(this).parent().attr("turnamentId");
        signalRChatHub.server.registerTurnament(turnamentId, myId);
    });

    $(document).on("click", '.unreg', function () {
        // Call Server method.
        var myId = device.uuid;
        var turnamentId = $(this).parent().attr("turnamentId");
        signalRChatHub.server.unRegisterTurnament(turnamentId, myId);

    });

    $(document).on("click", '.invite', function () {
        // Call Server method.
        var myId = device.uuid;
        var invitedUserId = $(this).attr("user-id");
       
        var amountSum = $(this).parent().parent().find(".input-invite").val();
        var rows = $("#tb-rows").val();
        var cols = $("#tb-cols").val();
        console.log("myId: " + myId)
        console.log("oponent: " + invitedUserId)
        console.log("rows: " + rows)
        console.log("cols: " + cols)
        signalRChatHub.server.inviteUser(myId, invitedUserId, rows, cols, amountSum);

    });

    signalRChatHub.client.inviteUser = function (myId, invitedUserId, rows, cols, amountSum) {
        var deviceId = device.uuid;
        if (device.uuid == invitedUserId) {
            var textInfo = "User:" + myId + ' invite you to play on table with ' + rows + ' rows and' + cols + ' cols.';
            if (amountSum != "" && amountSum != 0) {
                 textInfo = "User:" + myId + ' wat to play for' +amountSum+' $. <br/> on table with ' + rows + ' rows and' + cols + ' cols.';
            }
            $("#inviteUserDiv").find(".info").html(textInfo);
            $("#inviteUserDiv").attr("inviteMeUser", invitedUserId);
            $("#inviteUserDiv").attr("myId", myId);
            $("#inviteUserDiv").attr("rows", rows);
            $("#inviteUserDiv").attr("cols", cols);
            $("#inviteUserDiv").attr("amount", amountSum);
            $("#inviteUserDiv").show();
        }
    };




    signalRChatHub.client.inviteUser = function (myId, invitedUserId, rows, cols,amountSum) {
        var deviceId = device.uuid
        if (device.uuid == invitedUserId) {
            var textInfo = "User:" + myId + ' invite you to play on table with ' + rows + ' rows and' + cols + ' cols.';
            if (amountSum != "" && amountSum != 0) {
                textInfo = "User:" + myId + ' wat to play for' + amountSum + ' $. <br/> on table with ' + rows + ' rows and' + cols + ' cols.';
            }
            $("#inviteUserDiv").find(".info").html(textInfo);
            $("#inviteUserDiv").attr("inviteMeUser", invitedUserId);
            $("#inviteUserDiv").attr("myId", myId);
            $("#inviteUserDiv").attr("rows", rows);
            $("#inviteUserDiv").attr("cols", cols);
            $("#inviteUserDiv").attr("amount", amountSum);
            $("#inviteUserDiv").show();
        }
    };

    signalRChatHub.client.acceptInvite = function (myId, invitedUserId, rows, cols, tableId) {
        var deviceId = device.uuid
        if (myId == deviceId || invitedUserId == deviceId) {
            buildTable(rows, cols, tableId, myId, invitedUserId);
            $("#inviteUserDiv").hide();
            window.userTables.push(tableId);
        }
    };

    signalRChatHub.client.allertAMessage = function (myId, invitedUserId, message) {
        var deviceId = device.uuid
        if (myId == deviceId || invitedUserId == deviceId) {
            alert(message);
        }
    };

    signalRChatHub.client.consoleLog = function (message) {
        console.log(message);
    }

    signalRChatHub.client.allertAMessageOnly = function (message) {
        alert(message);
    };

    signalRChatHub.client.keepMeAlive = function () {
        console.log("I am alive");
        //for (i = 0; i < window.userTables.length; i++) {
        //    checkUserIsInGame(window.userTables[i]); 
        //}
    };

    signalRChatHub.client.userClientId = function (message) {
        //alert(message);
        userWebClientId = message;
        debugger;
        if (userWebClientId == undefined) {
            debugger;
            $.connection.hub.start();
        }
        console.log(userWebClientId);
    };

    $(document).on("click", '.line', function () {
        var lineId = $(this).attr("data-id");
        var table = $(this).parent();
        var tableId = $(table).attr("tbl");
        tableId = tableId.split('-')[1];
        var myId = $(table).attr("me");
        var oponent = $(table).attr("oponent");

        var myDevice = device.uuid;
        if (myDevice != myId) {

            oponent = myId;
            myId = myDevice;
        }

        signalRChatHub.server.makeMove(myId, oponent, tableId, lineId);
    });

    signalRChatHub.client.drawLine = function (myId, invitedUserId, tableId, lineId) {
        var deviceId = device.uuid;
        if ((myId == deviceId && window.hasCloseBox == false) ||
            (invitedUserId == deviceId && window.hasCloseBox == true)) {

            var tableIdInt = tableId;
            tableId = "table-" + tableId;
            drawLine1(tableId, lineId);

            $("a[aria-controls=" + tableId + "]").css("background", "white");
            $("a[aria-controls=" + tableId + "]").css("color", "black");
            //$("a[aria-controls=" + tableId + "]").find(".time-left").hide();
            $("a[aria-controls=" + tableId + "]").find(".time-left").text("30");

            setTimeout(function () {
                checkUserIsInGame(tableIdInt);
            }, 30000);
        }
        else if ((invitedUserId == deviceId && window.hasCloseBox == false) ||
                 (myId == deviceId && window.hasCloseBox == true)) {
            tableId = "table-" + tableId;
            drawLine1(tableId, lineId);

            $("a[aria-controls=" + tableId + "]").css("background", "green");
            $("a[aria-controls=" + tableId + "]").css("color", "white");
            $("a[aria-controls=" + tableId + "]").css("text-weight", "bold");
            $("a[aria-controls=" + tableId + "]").find(".time-left").text("30");
            $("a[aria-controls=" + tableId + "]").find(".time-left").show();

        }
        window.hasCloseBox = false;
    };

    signalRChatHub.client.colorBox = function (myId, invitedUserId, tableId, boxId) {
        window.hasCloseBox = true;
        var deviceId = device.uuid
        if (myId == deviceId) {

            tableId = "table-" + tableId;
            colorBox(tableId, boxId,"purple");

        }
        else if (invitedUserId == deviceId) {
            tableId = "table-" + tableId;
            colorBox(tableId, boxId, "green");

        }
    };

    function colorBox(tableId, boxId, color) {
        tableId = tableId.split('-')[1];
        var id = tableId + '-' + boxId;
        $("div[boxId='" + id + "'").css("background", color);
        $("div[boxId='" + id + "'").css("opacity", 1);
    }

    function drawLine1(tbl, id) {

        var tbl = tbl.split('-')[1];
        var lineId = tbl + "-" + id;
        $(".line[lineId='" + lineId + "'").css("background", "black");
        $(".line[lineId='" + lineId + "'").css("opacity", 1);
    }



    signalRChatHub.client.oponentOnTurn = function (myId) {
        var deviceId = device.uuid
        if (myId == deviceId) {
            alert("oponent on turn");
        }
    };

    
    signalRChatHub.client.userInfo = function (message) {
        window.userPPInfo=message;
    };

    signalRChatHub.client.userLogIn = function (message, username) {
        if (message == "failed") {
            alert("wrong username or password");
            $("#auth-page").show();
        }
        else {
            window.userPPInfo = device.uuid;
            window.username = username;
            $("#loginName").val(username);
            $('#loginform').find('#username').val("");
            $('#loginform').find('#password').val("");
            $(".tab-content").show();
            $(".nav-tabs").show();
            $("#auth-page").hide();
        }
    }

   
    // Click event-handler for broadcasting chat messages.
    $('#broadcast').click(function () {
        // Call Server method.
        signalRChatHub.server.send(device.uuid, $('#message').val());
        $('#message').val("");
    });



    // Click event-handler for clearing chat messages.
    $('#clear').click(function () {
        $('ul li').remove();
    });

    function checkUserIsInGame(gameId) {
        tableId = "table-" + gameId;
        var tableInfo = $("a[aria-controls=" + tableId + "]");
        var tableInfoColor = $(tableInfo).css("background");
        var secondLeft = $(tableInfo).find(".time-left").text();
        if (secondLeft <=0) {
            signalRChatHub.server.isInGame(gameId, device.uuid);
        }
    }

        signalRChatHub.client.gameOver = function (message){
            alert(message);
        }

        //$(document).on("click",".tab-pane-myprofile",function () {
        //    alert(10);
        //});  

        $("a[href='#myprofile']").click(function () {
            signalRChatHub.server.getUserFullInfo(device.uuid, window.username);
        });
        
        signalRChatHub.client.getUserFullInfo = function (user) {
            $("#user-profile").find("#username").text(user.Username);
            $("#user-profile").find("#amount").text(user.Amount + "$");
        }


var app = {
    // Application Constructor
    initialize: function () {
        this.bindEvents();
    },
    // Bind Event Listeners
    //
    // Bind any events that are required on startup. Common events are:
    // 'load', 'deviceready', 'offline', and 'online'.
    bindEvents: function () {
        document.addEventListener('deviceready', this.onDeviceReady, false);
    },
    // deviceready Event Handler
    //
    // The scope of 'this' is the event. In order to call the 'receivedEvent'
    // function, we must explicity call 'app.receivedEvent(...);'
    onDeviceReady: function () {
        app.receivedEvent('deviceready');
    },
    // Update DOM on a Received Event
    receivedEvent: function (id) {
       // var parentElement = document.getElementById(id);
       // var listeningElement = parentElement.querySelector('.listening');
       // var receivedElement = parentElement.querySelector('.received');
       //
       // listeningElement.setAttribute('style', 'display:none;');
       // receivedElement.setAttribute('style', 'display:block;');

        console.log('Received Event: ' + id);

        // start to initialize PayPalMobile library
        app.initPaymentUI();
    },
    initPaymentUI: function () {
        var clientIDs = {
            "PayPalEnvironmentProduction": "Ae6h1211KMBrKUfp_qt5JLvEVd1WPbh_UGB7N8wTZr9W1ycJU-8uRvxzmsh4XfqMIN--VOZ9OT1Gusqt",
            "PayPalEnvironmentSandbox": "AV70EMk-xsESvLoWbC4GKkbxnFJCSKPhZmtbumTZ7lW-9LlSTVj1jwFnNU1Va9saEXeUbQH2TWxWze0c"
        };
        PayPalMobile.init(clientIDs, app.onPayPalMobileInit);

    },
    onSuccesfulPayment: function (payment) {
        signalRChatHub.server.addAmount(device.uuid, payment.response.id, window.amount);
        console.log("payment amount: " + window.amount);
        console.log("payment success: " + JSON.stringify(payment, null, 4));
        window.amount = null;
    },
    onAuthorizationCallback: function (authorization) {
        console.log("authorization: " + JSON.stringify(authorization, null, 4));
    },
    createPayment: function (amount) {
        // for simplicity use predefined amount
        // optional payment details for more information check [helper js file](https://github.com/paypal/PayPal-Cordova-Plugin/blob/master/www/paypal-mobile-js-helper.js)
        var paymentDetails = new PayPalPaymentDetails(amount, "0.00", "0.00");
        
        var payment = new PayPalPayment(amount, "USD", window.userPPInfo + ": Charge account", "Sale", paymentDetails);
        window.amount = amount;
        return payment;
    },

    createPaymentDump: function (amount) {
        var payment = {
            "response": {
                "state": "approved",
                "id": "PAY-8TK70833VC2155120KXYXAVA",
                "create_time": "2015-09-10T11:58:12Z",
                "intent": "sale"
            },
            "client": {
                "platform": "Android",
                "paypal_sdk_version": "2.8.8",
                "product_name": "PayPal-Android-SDK",
                "environment": "live"
            },
            "response_type": "payment"
        };
        return payment;
    },

    configuration: function () {
        // for more options see `paypal-mobile-js-helper.js`
        var config = new PayPalConfiguration(
            {
                merchantName: "Dots shop",
                acceptCreditCards: true,
                merchantPrivacyPolicyURL: "https://mytestshop.com/policy",
                merchantUserAgreementURL: "https://mytestshop.com/agreement" 
                
            });
        return config;
    },
    onPrepareRender: function () {
        // buttons defined in index.html
        //  <button id="buyNowBtn"> Buy Now !</button>
        //  <button id="buyInFutureBtn"> Pay in Future !</button>
        //  <button id="profileSharingBtn"> ProfileSharing !</button>
        var buyNowBtn = document.getElementById("buyNowBtn");
        var buyInFutureBtn = document.getElementById("buyInFutureBtn");
        var profileSharingBtn = document.getElementById("profileSharingBtn");

       
     /*   buyNowBtn.onclick = function (e) {
            // single payment
            var amount = $("#amount").val();
            debugger;
            PayPalMobile.renderSinglePaymentUI(app.createPaymentDump(amount), app.onSuccesfulPayment, app.onUserCanceled);
        };*/

        buyNowBtn.onclick = function (e) {
            // single payment
            var amount = $("#amount").val();
            PayPalMobile.renderSinglePaymentUI(app.createPayment(amount), app.onSuccesfulPayment, app.onUserCanceled);
        };

        buyInFutureBtn.onclick = function buyInFutureBtn(e) {
            // future payment
            PayPalMobile.renderFuturePaymentUI(app.onAuthorizationCallback, app.onUserCanceled);
        };

        profileSharingBtn.onclick = function (e) {
            // profile sharing
            PayPalMobile.renderProfileSharingUI(["profile", "email", "phone", "address", "futurepayments", "paypalattributes"], app.onAuthorizationCallback, app.onUserCanceled);
        };
    },

    buyNow: function () {
        var amount = $("#amount").val();
        // single payment
        PayPalMobile.renderSinglePaymentUI(app.createPayment(), app.onSuccesfulPayment, app.onUserCanceled);
    },

    onPayPalMobileInit: function () {
        // must be called
        // use PayPalEnvironmentNoNetwork mode to get look and feel of the flow
        PayPalMobile.prepareToRender("PayPalEnvironmentProduction", app.configuration(), app.onPrepareRender);
    },
    onUserCanceled: function (result) {
        console.log(result);
    }
};

app.initialize();

});
