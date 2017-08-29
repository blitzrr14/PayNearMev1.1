/*Send Money Step one Uses Java script Start here*/
var locale=$("#localeValue").val();
var defaultFirst = true;
var isfirstverified=false;
var issecondverified=false;
var isthirdverified=false;
var floatRegEx= /^[0-9]{0,4}[.]?[0-9]{0,2}$/;
var sFee="";
var addSendingMethodUrl="";
var editSendingMethodUrl="";
var sendingMethodName="";
var bankAccSfee="";
var CreditCardSfee=""; 
var DebitCardSfee="";
var payModeArr="";
var defaultRadioSelect="";
var ALL_SER_FEE_OBJECT="";
var defaultPaymodeId="";
var sFeeBeforHitCore="";

var ajaxRequest;
function ajaxFunction(){
// The variable that makes Ajax possible!
//  alert("In making ajax request");
try
{
// Opera 8.0+, Firefox, Safari
ajaxRequest = new XMLHttpRequest();
}
catch (e)
{
// Internet Explorer Browsers
try
{
	 ajaxRequest = new ActiveXObject("Msxml2.XMLHTTP");
}
catch (e) 
{
	 try
	 {
		         ajaxRequest = new ActiveXObject("Microsoft.XMLHTTP");
		      }
	 catch (e)
	 {
		         // Something went wrong
		         customAlert("Your browser broke!");
		         //return false;
	 }
}
}
}

function checkBeneficiaryList()
{ 
	   
	  	var selectedBeneId =document.getElementById("beneId")[document.getElementById("beneId").selectedIndex].value;  
    	var amt=$('#sendingAmount').val();
    	var limitCheck = document.getElementById("limitCheck").value;
    	defaultPaymodeId="";
    	sFeeBeforHitCore="";
    	if(selectedBeneId=='')
    		{ 
    		     allSetBydefaultVal(); 
    		}else
    		{
    			changeExrateOnchangeBeneficiary(selectedBeneId,limitCheck);
    			checkAdditionalField(selectedBeneId);
    		}  
} 
 
    
function changeSendAmountToReceipientReceives()
{
	 defaultPaymodeId="";
	 var selectedBeneId =document.getElementById("beneId")[document.getElementById("beneId").selectedIndex].value;
	 var sAmount=document.getElementById("sendingAmount").value; 
	 sAmount=removeCommaSeperatorFromAmount(sAmount);
	 var limitCheck = document.getElementById("limitCheck").value;
	 if(selectedBeneId=='')
		{  
          var beneSelectMsg=document.getElementById("label.selectBeneficiary").value;
          customErrorModal(beneSelectMsg);
          document.getElementById("sendingAmount").value="";
          return false;
		}else if(validateSendAmount(sAmount,limitCheck))
	    {
			fillTransferSummary(sAmount,true);
			 
		}else
		{
			return false;
		}
	 
}

function changeReceipientReceivesToSendAmount()
{
	 defaultPaymodeId=""
	 var selectedBeneId =document.getElementById("beneId")[document.getElementById("beneId").selectedIndex].value;
	 var rRecipient=document.getElementById("recipientReceivestextId").value; 
	 var defautlPaymentModeName=document.getElementById("defautlPaymentModeName").value; 
	 var limitCheck = document.getElementById("limitCheck").value;
	 var recWithoutComma = removeCommaSeperatorFromAmount(rRecipient);
	 if(selectedBeneId=='')
		{   
	       var beneSelectMsg=document.getElementById("label.selectBeneficiary").value;
	       customErrorModal(beneSelectMsg);
	       document.getElementById("recipientReceivestextId").value="";
		}else if(recWithoutComma == '')
		{ 
			  document.getElementById("recipientReceivestextId").value="";
			  document.getElementById("sendingAmount").value="";
			  $('#youRSend').text("00.00 USD");
			  $('#sFee').text("00.00 USD"); 
			  $('#totalAmt').text("00.00 USD"); 
			  $('#RecipientReceives').text("00.00 USD");
			  /*$('#payerStep1').empty(); 
			  $('#RecipientReceivesOpt').empty();*/ 
			  
	    }else if(isNaN(recWithoutComma))
		{ 
	    	  
			  document.getElementById("recipientReceivestextId").value="";
			  document.getElementById("sendingAmount").value="";
			  $('#youRSend').text("00.00 USD");
			  $('#sFee').text("00.00 USD"); 
			  $('#totalAmt').text("00.00 USD"); 
			  $('#RecipientReceives').text("00.00 USD");
			  
	    }else if(recWithoutComma.length > 15)
		{
	    	  $("#recipientReceivestextId").blur();
			  var validRecepientAmt= document.getElementById("label.validRecepientAmount").value;
			  customErrorModal(validRecepientAmt);
			  document.getElementById("recipientReceivestextId").value="";
			  document.getElementById("sendingAmount").value="";
			  $('#youRSend').text("00.00 USD");
			  $('#sFee').text("00.00 USD"); 
			  $('#totalAmt').text("00.00 USD"); 
			  $('#RecipientReceives').text("00.00 USD"); 
	    }else	    		 
	    { 	 
	    	 var exrate= document.getElementById("exchangeRate").value;
	    	 var sendAmount=(parseFloat(recWithoutComma)/parseFloat(exrate)).toFixed(2);
			 if(validateSendAmount(sendAmount,limitCheck))
			 {
				 fillTransferSummary(recWithoutComma,false);
			 } 
	    	 
	    }
}

function fillTransferSummary(sendAmt,isSendAmt)
    {
			
 
         var cur='USD';
         var sendAmount="";
         var receiveAmt="";
         var exchangeCurr=$('#exchangeCurrency').text();
         exchangeCurr=" "+exchangeCurr;
         var exrate= document.getElementById("exchangeRate").value;
         if(isSendAmt)
         {
        	 sendAmount=parseFloat(sendAmt);
        	 receiveAmt=(sendAmount * parseFloat(exrate));
        	 sendAmount=parseFloat(sendAmount).toFixed(2);
        	 receiveAmt=parseFloat(receiveAmt).toFixed(2);
        	 $('#RecipientReceives').text(addCommaSep(receiveAmt,true)+""+exchangeCurr); 
         }else
         {
        	 receiveAmt=(parseFloat(sendAmt));
        	 sendAmount=(receiveAmt/parseFloat(exrate))
        	 sendAmount=parseFloat(sendAmount).toFixed(2);
        	 document.getElementById("sendingAmount").value=sendAmount;
        	 $('#RecipientReceives').text(addCommaSep(receiveAmt.toFixed(2),true)+""+exchangeCurr); 
         }        
         
         sFee=getTxSummeryByAmt(sendAmount);
         sFeeBeforHitCore=sFee;
         
         if(null=== sFee || '' === sFee || 'undefined' === sFee || "" === sFee)
          { 
        	 $('#youRSend').text("00.00 USD");
			 $('#sFee').text("00.00 USD"); 
			 $('#totalAmt').text("00.00 USD"); 
			 $('#RecipientReceives').text("00.00 USD");
			 $('#sendingAmount').val('');
			 $('#recipientReceivestextId').val('');
			 $("#sendingAmount").blur();
        	 customErrorModal(document.getElementById("label.serviceFeeNotFountForPayer").value);
        	 return false;
          }
         
         var floatsee=getTxSummeryByAmt(sendAmount);       
         var total= (parseFloat(sendAmount)+ parseFloat(floatsee)).toFixed(2) ;  
         total=addCommaSep(total,true)+" "+cur;
         sendAmount=addCommaSep(sendAmount,true)+" "+cur;
         $('#youRSend').text(sendAmount);
         $('#sFee').text(sFee+" "+cur);  
         $('#totalAmt').text(total); 
         
		 var exRate=document.getElementById("exchangeRate").value;	 
		 document.getElementById("recipientReceivestextId").value=addCommaSep(parseFloat(receiveAmt),true); 
         // sync same data for step 2 transfer summery         
    	 $('#youRSendStep2').text(sendAmount);
         $('#sFeeStep2').text(sFee+" "+cur); 
         $('#totalAmtStep2').text(total); 
         $('#RecipientReceivesStep2').text(addCommaSep(parseFloat(receiveAmt).toFixed(2),true));
         $('#exchangeRateStep2').text("1 "+cur+" = "+exrate+" "+exchangeCurr);  
         
	             
 } 
 
 function validateSendAmount(sAmount,limitCheck)
 {
	   var isValid=true; 
	   if(sAmount == '')
		{ 
			  document.getElementById("sendingAmount").value="";
			  document.getElementById("recipientReceivestextId").value="";
			  $('#youRSend').text("00.00 USD");
			  $('#sFee').text("00.00 USD"); 
			  $('#totalAmt').text("00.00 USD"); 
			  $('#RecipientReceives').text("00.00 USD"); 
			  isValid= false;
	    }else if(isNaN(sAmount))
		{  
			  document.getElementById("sendingAmount").value=""; 
			  document.getElementById("recipientReceivestextId").value="";
			  $('#youRSend').text("00.00 USD");
			  $('#sFee').text("00.00 USD"); 
			  $('#totalAmt').text("00.00 USD"); 
			  $('#RecipientReceives').text("00.00 USD"); 
			  isValid= false;
	    }else if(!(floatRegEx.test(sAmount)))
		{
	    	  $("#sendingAmount").blur();
			  var validSendingAmt= document.getElementById("label.validSendingAmt").value;
			  customErrorModal(validSendingAmt);
			  document.getElementById("sendingAmount").value="";
			  document.getElementById("recipientReceivestextId").value="";
			  $('#youRSend').text("00.00 USD");
			  $('#sFee').text("00.00 USD"); 
			  $('#totalAmt').text("00.00 USD"); 
			  $('#RecipientReceives').text("00.00 USD"); 
			  isValid= false; 
		}else if(parseFloat(sAmount) <= 0.0 || parseFloat(sAmount) > parseFloat(limitCheck))
		{
			  document.getElementById("sendingAmount").value="";
			  document.getElementById("recipientReceivestextId").value="";
			  $('#youRSend').text("00.00 USD");
			  $('#sFee').text("00.00 USD"); 
			  $('#totalAmt').text("00.00 USD"); 
			  $('#RecipientReceives').text("00.00 USD"); 
			  var limitMsg1= document.getElementById("label.sendMoney1").value;  
		      var limitMsg2= document.getElementById("label.sendMoney5").value;
		      var finalMsg=limitMsg1+" "+parseFloat(limitCheck).toFixed(2)+" "+limitMsg2+"<br>";
			  customErrorModal(finalMsg);
			  isValid= false;
		}
	   
	   return isValid;
 }

function changeExrateOnchangeBeneficiary(selectedBeneId,limitCheck)
{
	
	if((null!=selectedBeneId && ''!=selectedBeneId))
	{ 
		var tokenSecurity=$("#tokenSecurity").val();
		var  uniseccodenc = $("#uniseccodenc").val();
	    //loadingStart();
		ajxloadtoggle();
		   $.ajax({ 
   	       url: "fetchExchangeRate.action?request_locale="+locale, 
   	       headers: {"Content-type":"application/x-www-form-urlencoded"},
   	       type: 'POST',
   	       data:"beneficiaryId="+selectedBeneId+"&tokenSecurity="+tokenSecurity+"&uniseccodenc="+uniseccodenc+"&dailyLimit="+limitCheck,
   	       dataType: 'json',
   	       success:function(jsonresponse) 
   	       { 
   	           //loadingEnd();
   	           ajxloadtoggle();
   	    	  $("#tokenSecurity").val(jsonresponse.tokenSecurity);  
   	    	  var errorMsg=jsonresponse.errorMessage; 
   	    	  if(null!=errorMsg && errorMsg!='')
   	          {
   	    		      customErrorModal(errorMsg); 
   	    	  }else
   	    	  {
   	    		      var EX_RATE=jsonresponse.exRateNadSfee.EXRATE;
     	    	      var ALL_SER_FEE=jsonresponse.exRateNadSfee.SER_FEE;
     	    	      ALL_SER_FEE_OBJECT=ALL_SER_FEE;
   	    			  var cur='USD';
   	    			  $('#youRSend').text("00.00 USD");
   	    			  $('#sFee').text("00.00 USD");  
   	    			  $('#totalAmt').text("00.00 USD"); 
   	    			  $('#RecipientReceives').text("00.00 USD"); 
   	    			  document.getElementById("sendingAmount").value="";
   	    			  document.getElementById("recipientReceivestextId").value="";
   	    			
		   	    	  $('#exchangeCurrency').text(jsonresponse.exRateNadSfee.BENE_CURRENCY); 
		   	          document.getElementById("exchangeRate").value=EX_RATE;  
		   	          $('#beneName').text(jsonresponse.exRateNadSfee.BENE_NAME);
		   	          $('.exchangeRate').text("1 "+cur+" = "+EX_RATE+" "+jsonresponse.exRateNadSfee.BENE_CURRENCY); 
		   	          $('#payerStep1').text(jsonresponse.exRateNadSfee.PAYER_NAME);
		   	          $('#RecipientReceivesOpt').text(jsonresponse.exRateNadSfee.RECEPTION_MODE);
		   	          //Fill second step..
		   	          $('#beneNameStep2').text(jsonresponse.exRateNadSfee.BENE_NAME);
		   	          $('#payerStep2').text(jsonresponse.exRateNadSfee.PAYER_NAME);
		              $('#RecipientReceivesOptStep2').text(jsonresponse.exRateNadSfee.RECEPTION_MODE);
   	    	  } 
   	    	  
   	       },error: function(jqXHR, exception) 
 	       {  
   	           //loadingEnd(); // End loading div
   	           ajxloadtoggle();
   	 	     sessionTimeOutGoBackLoginScreen(jqXHR);
   	       } 
   	      }); 

     }else
     {
    	 loadingEnd();
    	 $('#exchangeCurrency').text(''); 
    	 document.getElementById("exchangeRate").value="";
    	 $('#payerStep1').empty(); 
		 $('#RecipientReceivesOpt').empty(); 
     }	 
	 
	/*if(null!=sendAmount && ''!=sendAmount && 'undefined'!=sendAmount)
	{
		var sendAmt=parseFloat(sendAmount) ;
	    sendAmt=sendAmt.toFixed(2); 
	    fillTransferSummary(selectedBeneId,sendAmt);
	
	}*/
	
	
}

function sendMoneyPayment()
{
	  var beneId = document.getElementById("beneId").value;
	  var amount = document.getElementById("sendingAmount").value; 
	  var limitCheck = document.getElementById("limitCheck").value; 
	  var errorFlag=false;
	  var errorMsg=""; 
	  defaultRadioSelect="";
	  var errorFields = new Array();
	  
	  if(null == beneId || beneId == '')
       {
			 var beneSelectMsg= document.getElementById("label.selectBeneficiary").value; 
			 errorMsg=errorMsg+beneSelectMsg+"<br>";
			 errorFlag=true; 
	   }
	  if(null== amount || amount == '')
	   {
			 var amontError= document.getElementById("label.sendAmountMsg").value; 
			 errorMsg=errorMsg+amontError+"<br>";
			 errorFlag=true;
		}else if(!(floatRegEx.test(amount)))
		{
			var validSendingAmt= document.getElementById("label.validSendingAmt").value;
			errorMsg=errorMsg+validSendingAmt+"<br>";
			errorFlag=true; 
		}else if(amount < 25.00 || amount == '0.00')
		{
			  var maxSendAmountMsg=document.getElementById("label.maxSendingAmountMsg").value;
			  errorMsg=errorMsg+maxSendAmountMsg+"<br>";
			  errorFlag=true;
		}  
	   
	    if(null!=amount && ''!=amount && parseFloat(amount) > parseFloat(limitCheck))
		{
	    	 var limitMsg1= document.getElementById("label.sendMoney1").value;  
	    	 var limitMsg2= document.getElementById("label.sendMoney5").value;
	    	 var finalMsg=limitMsg1+" "+parseFloat(limitCheck).toFixed(2)+" "+limitMsg2+"<br>";
			 errorMsg+=finalMsg;
			 errorFlag=true; 
	    }	 
		
	   if(errorFlag)
		   {
		    customErrorModal(errorMsg); 
		    return false;
		   }
	   else
	   { 
		   var tokenSecurity=$("#tokenSecurity").val();
		   var 	parameter="beneficiaryId="+beneId+"&isSendMoney=true&sendingAmount="+amount+"&tokenSecurity="+tokenSecurity+"&defaultPayModeID="+defaultPaymodeId;    
		   loadingStart(); // start loading div
		   $.ajax({ 
	   	       url: "sendMoneyStep1_New.action?request_locale="+locale, 
	   	       headers: {"Content-type":"application/x-www-form-urlencoded"},
	   	       type: 'POST',
	   	       data:parameter,
	   	       dataType: 'json',
	   	       success:function(sendMoneyStep1Response) 
	   	       {
	   	    	try{hj('trigger','send_step_1');}catch(e){console.log("error in hj('trigger','send_step_1')");}   
	   	    	$("#tokenSecurity").val(sendMoneyStep1Response.tokenSecurity);
	   	    	  //loadingEnd(); // End loading div
	   	    	  var errorMsg=sendMoneyStep1Response.errorMsgString;
	   	    	  var infoMsg=sendMoneyStep1Response.infoMsgString;
	   	    	  var txAFReqPopUp=sendMoneyStep1Response.txAFReqPopUp;
	   	    	  var errorResponseOnConfirm=sendMoneyStep1Response.errorResponseOnConfirm;
	   	    	  if(null!=errorResponseOnConfirm && ''!=errorResponseOnConfirm && 'undefined'!=errorResponseOnConfirm && 'dataTempered'==errorResponseOnConfirm)
	   	    	  {
	   	    		  window.location.href="LogoutAction.action?request_locale="+locale;
	   	    		  return false;
	   	    	  }
	   	    	    
	   	    	  loadingEnd(); // End loading div
	   	    	  
	   	    	  if(null!=errorMsg && errorMsg!='')
	   	    		  {
	   	    		      customErrorModal(errorMsg); 
	   	    		  }else if(null!=infoMsg && infoMsg!='')
	   	    			  {
		   	    			  if(txAFReqPopUp!=null && txAFReqPopUp!='' && "YES"==txAFReqPopUp)
		   	    			  { 
		   	    				showAllAdditionalFieldForEdit();
		   	    			  }else{
		   	    				customInfoModal(infoMsg);
		   	    			  }
	   	    			        
	   	    			  }else
	   	    				  {  
		   	    				if(null==defaultRadioSelect || '' == defaultRadioSelect)
	  	    				     {
		   	    				  defaultRadioSelect=sendMoneyStep1Response.defaultRadioSelect; 
	  	    				     }	 
	   	    				     var sfeeResponce = JSON.parse(sendMoneyStep1Response.responceJson); 
	   	    				     var payModeList=sendMoneyStep1Response.paymentModeList; 
	   	    				     gotoSendStep2(payModeList,defaultRadioSelect,sfeeResponce);
	   	    				  }
	   	    	 
	   	       },
	   	       error: function(jqXHR, exception) 
	   	        {  
	   	    	 loadingEnd(); // End loading div
	   	    	sessionTimeOutGoBackLoginScreen(jqXHR);
	            }
		       
	   	      });  
	    }  		   
	   
	 
}  

 function gotoSendStep2(payModeList,defaultRadioSelect,serviceFeeResponse)
 
{ 	  
	 fillAllDetailsOfSendStepTwo(payModeList,defaultRadioSelect,serviceFeeResponse); 
	 openaccordion("paymentbodytitle");
	 scrollTop(100);
} 
 
 function padStar(str)
 { 
	 if (null == str || str == '')
			{
			 return "";
			}else 
		   {
			    var localStr = "";
				var paddingStar = "*****";
				localStr=paddingStar+(str.substring(str.length-4, str.length)); 
				return localStr;
		   }
}
 
 function sendMoneyStep2()
  { 	 
	  issecondverified =false;
	 var paymentModeRadios = document.getElementsByName("paymentModeId"); 
	 var radioVal="";
	 for(var i = 0; i < paymentModeRadios.length; i++ ) 
	   {
	        if( paymentModeRadios[i].checked ) 
	        {   
	        	 radioVal=paymentModeRadios[i].value;
	        	 break;
	        } 
	   }
		 
	  if(radioVal == '')
	  {
		  var noSendingMethodError=	document.getElementById("label.PleaseSelectSendingMethod").value;
          customErrorModal(noSendingMethodError);  
          return false;
      }
	  
	  loadingStart(); // start loading div
	  var tokenSecurity=$("#tokenSecurity").val();
	  
	  var 	parameter="paymentModeId="+radioVal+"&tokenSecurity="+tokenSecurity+"&serviceFee="+sFeeBeforHitCore;    
	   $.ajax({ 
  	       url: "sendMoneyStep2_New.action?request_locale="+locale, 
  	       headers: {"Content-type":"application/x-www-form-urlencoded"},
  	       type: 'POST',
  	       data:parameter, 
  	       dataType: 'json',
  	       success:function(sendMoneyStep2Response) 
  	       { 
  	    	 try{hj('trigger','send_step_2');}catch(e){console.log("error in hj('trigger','send_step_2')");}   
  	    	   
  	    	  $("#tokenSecurity").val(sendMoneyStep2Response.tokenSecurity);
  	    	  loadingEnd(); // End loading div
  	    	  var errorMsg=sendMoneyStep2Response.errorMsgString;
  	    	  var infoMsg=sendMoneyStep2Response.infoMsgString;
  	    	  if(null!=errorMsg && errorMsg!='')
	    		  {
	    		      customErrorModal(errorMsg); 
	    		  }else if(null!=infoMsg && infoMsg!='')
	    			  {
	    			     openaccordion("beneficiarybodytitle");
	    			     customInfoModal(infoMsg);  
	    			  }else
	    				  {
	    				     var responceObj = JSON.parse(sendMoneyStep2Response.responceJson);
	    				     var feeNRateChangeFlag=sendMoneyStep2Response.exchangeRateNserviceFeeChnageMsg;
	    				     gotoSendStep3(responceObj,feeNRateChangeFlag); 
	    				  }
  	       },
  	       error: function(jqXHR, exception) 
  	        {  
  	    	   
  	    	 loadingEnd(); // End loading div
  	    	sessionTimeOutGoBackLoginScreen(jqXHR);
           }
	       
  	      });  
	}
 
 function gotoSendStep3(responceObj,feeNRateChangeFlag)
 {   
	 if(null==responceObj || ''==responceObj || 'undefined'==responceObj)
	 {
		 customErrorModal('An error has occurred. Please try again at a later time. If error persists please contact Customer Service.');
		 return false;
	 }else
     {
	 	  $('#sendAmtStep3').text(addCommaSep(responceObj.sendAmount,true));
	 	  $('#transferFeeStep3').text(responceObj.transactionFee);
	      $('#eRateStep3').text(responceObj.exchangeRate); 
	      $('#totalAmtStep3').text(addCommaSep(responceObj.total,true)); 
	      $('#recivingAmtStep3').text(addCommaSep(responceObj.receivingAmount,true));
	      $('#amountChargeStep3').text(addCommaSep(responceObj.amtCharge,true));  
	      $('#beneNameStep3').text(responceObj.beniFullName);  
	      $('#beneAddressStep3').text(responceObj.beneAddress1);  
	      $('#benePhoneStep3').text(responceObj.beneCellPhone); 
	      $('#receptionMethodStep3').text(responceObj.receptionMethod); 
	      $('#payerNameStep3').text(responceObj.payerName); 
	      $('#payModeStep3').text(getTextCustom(responceObj.paymentMode)); 
	      $('#payModeCardNoStep3').text(responceObj.cardNo); 
	      $('#promoCodeErrorShowDivId').empty(); 
	      var applyButton="<button type='button' id='promoCodeButton' class='btn btn-success btn-apply pull-right' name='promoCodeButton' onclick='validatAndApplyPromoCode();'>Apply</button>";
	  	  $('#applyPromoButtonDivId').html(applyButton); 
	  	  
	      var holdMsg=responceObj.holdTransactionMsg; 
	      if(!(!holdMsg) && holdMsg!='undefied')
	      {
	        $('#holdTransactionMsg').text(holdMsg); 
	    	$('#holdTransactionMsgDiv').show();
	      }else
	      {
	    	$('#holdTransactionMsgDiv').hide();  
	      }	  
	      
	      issecondverified =true;   
	      if(null!=feeNRateChangeFlag && feeNRateChangeFlag!='' && 'undefined'!=feeNRateChangeFlag)
		  {  
	         customServiceFeeConfirmModal(feeNRateChangeFlag);   
		  }
	      {
			 openaccordion('confirmationtitle');  
	      }   
	      
	      scrollTop(130);
	 }
 } 
   
 
   function confirmTransaction()
  {  
	    loadingStart();
	    gotoConfirmationTransactionScreen();  
 }
 
 function goStep1()
 {  
	 openaccordion("beneficiarybodytitle");
 }
 function goStep2()
 {  
	 openaccordion("paymentbodytitle");
 }
  
	
//	validate accordion before open
	
	function beforeOpenAccordion(event,panelId )
	{
	 
		var divId = panelId;
// 	alert("id===="+divId);
		
		// to prevent self toggle accordian (at least one accordian always open)
 	 
		if($("#"+panelId).parents('.panel').children('.panel-collapse').hasClass('in')){
			event.stopPropagation();
			 event.preventDefault();
	    }		
		
		if(divId=='beneficiarybodytitle')
			{
			isfirstverified =false;
			issecondverified=false;
			reSetPromotionByDefaultVal();
			if(!defaultFirst){
				 event.stopPropagation();
				 event.preventDefault();
			}
			
			$("#beneficiarybodytitle span.steps-title").removeClass("disable");
			 $("#paymentbodytitle span.steps-title").removeClass("disable");
			}
		else
			{	
				if(divId=='paymentbodytitle')
							{
					         reSetPromotionByDefaultVal(); 
							 if(!isfirstverified)
							 {
								 event.stopPropagation();
								 event.preventDefault();
							 }
							 else{
								 $("#beneficiarybodytitle span.steps-title").addClass("disable");
								 $("#paymentbodytitle span.steps-title").removeClass("disable");
							 }
							 
							 if(issecondverified) issecondverified=false;  // while 2nd  time click on this.
							 
							}
				else{	
						if(divId=='confirmationtitle')
						{
							if(!isfirstverified || !issecondverified){
								 event.stopPropagation();
								 event.preventDefault();
							}
							else{
								 $("#paymentbodytitle span.steps-title").addClass("disable");
							 }
					
						}
				}
			}
		
		 
	}
	
function validatAndApplyPromoCode()
{
	    var promoCodeVal=$('#applyPromoCode').val();
	    if(!(!promoCodeVal) || promoCodeVal!='')
	    {
	    	var tokenSecurity=$("#tokenSecurity").val();
	    	var  uniseccodenc = $("#uniseccodenc").val();
	    	loadingStart();
		 $.ajax({
			        type: 'POST',
		            url: 'validateApplyPromoCode.action?request_locale='+locale,
		            headers: {"Content-type":"application/x-www-form-urlencoded"},
		            data : "promoCouponCode="+$('#applyPromoCode').val()+"&tokenSecurity="+tokenSecurity+"&uniseccodenc="+uniseccodenc,
		            dataType: 'json',
		            success: function(jsonResponse)
		            {   
		            	  loadingEnd();
		            	  var errorResponse=jsonResponse.response.ERROR;
		            	  if(!(!errorResponse) || errorResponse!='')
		            		 {
		            		   $('#promoCodeErrorShowDivId').text(jsonResponse.response.ERROR);
		            		   $('.hideForPrmo').hide();
		            		   $('#discountOnServiceFee').text("00.00 USD");
		            		   $('#actualFee').text("00.00 USD");
		            		   $('#totalAfertDiscountAmt').text("00.00 USD");  
		            		   $('#applyPromoCode').val('');  		            		            		   
		            		 }else
		            			 {
		            			 
		            			  $('.hideForPrmo').show();  
		            		      $('#discountOnServiceFee').text(jsonResponse.strDiscountOnServiceFee+" USD");
		            			  $('#actualFee').text(jsonResponse.serviceFeeAfterDiscount+" USD");
		            			  $('#totalAfertDiscountAmt').text(jsonResponse.totalAmtAfterDiscount+" USD"); 
		            			  $('#amountChargeStep3').text(jsonResponse.totalAmtAfterDiscount+" USD");
		            			  $('#promoCodeErrorShowDivId').html(jsonResponse.response.VALID);
		            			  var removeButton="<button type='button' id='removePromoCodeButton' class='btn btn-success btn-apply pull-right' name='removePromoCodeButton' onclick='removedApplyPromoCode();'>Remove</button>";
		            			  $('#applyPromoButtonDivId').html(removeButton); 
		            			  $('#applyPromoCode').attr('readonly','readonly');
		            			  /* $("#promoCodeButton").prop('disabled', true); */ 
		            			  
		            			 }
		            	 
		            	
		            	 
		            },
		    	    error: function(jqXHR, exception) 
		    	    {   
		    	    	 loadingEnd();
		    	    	 sessionTimeOutGoBackLoginScreen(jqXHR);

		            }
		         
		   });
	      }
	      else
	   		 { 
		    	  loadingEnd();
		    	  $('#promoCodeErrorShowDivId').text(document.getElementById("label.enterValidPromoCode").value);
		   		  $('#applyPromoCode').focus();
		   		  $('.hideForPrmo').hide();
		   		  $('#discountOnServiceFee').text("00.00 USD");
		   		  $('#actualFee').text("00.00 USD");
		   		  $('#totalAfertDiscountAmt').text("00.00 USD"); 
		   		  return false; 
	   		 }
			  
	    
}


function removedApplyPromoCode()
{
	
	loadingStart();
	var tokenSecurity=$("#tokenSecurity").val();
	var  uniseccodenc = $("#uniseccodenc").val();
	$.ajax({
        type: 'POST',
        url: 'removedAppliedPromoCode.action?request_locale='+locale,
        headers: {"Content-type":"application/x-www-form-urlencoded"},
        data : "promoCouponCode="+$('#applyPromoCode').val()+"&tokenSecurity="+tokenSecurity+"&uniseccodenc="+uniseccodenc,
        dataType: 'json',
        success: function(jsonResponse)
        {    
        	
        	loadingEnd();
            $('.hideForPrmo').hide();
        	$('#discountOnServiceFee').text("00.00 USD");
        	$('#actualFee').text("00.00 USD");
        	$('#totalAfertDiscountAmt').text("00.00 USD");
        	$('#promoCodeErrorShowDivId').empty();
        	$('#amountChargeStep3').text(jsonResponse.totalamount+" USD");
        	$('#applyPromoCode').removeAttr("readonly");
        	$('#applyPromoCode').val('');
        	var applyButton="<button type='button' id='promoCodeButton' class='btn btn-success btn-apply pull-right' name='promoCodeButton' onclick='validatAndApplyPromoCode();'>Apply</button>";
        	$('#applyPromoButtonDivId').html(applyButton); 
        	 
        },
	    error: function(jqXHR, exception) 
	    {   
	    	 loadingEnd();
	    	 sessionTimeOutGoBackLoginScreen(jqXHR);
        }
     
}); 

}
 

function allSetBydefaultVal()
{  
	$('#paymentModeDivId').empty();
	$('#beneName').empty();
    $('#youRSend').text("00.00 USD");
    $('#sFee').text("00.00 USD");
    $('#ReceivingOption').empty();
    $('#pName').empty();
    $('#totalAmt').text("00.00 USD");
    $('.exchangeRate').text("1 USD = 1.00 USD");
    $('#RecipientReceives').text("00.00 USD");
    document.getElementById("sendingAmount").value="";
    document.getElementById("recipientReceivestextId").value="";
    $('#exchangeCurrency').text("USD"); 
    $('#sendAmtStep3').text("00.00 USD");
	$('#transferFeeStep3').text("00.00 USD");
    $('#eRateStep3').text("1 USD = 1.00 USD"); 
    $('#totalAmtStep3').text("00.00 USD"); 
    $('#recivingAmtStep3').text("00.00 USD");
    $('#amountChargeStep3').text("00.00 USD");  
    $('#beneNameStep3').empty();  
    $('#beneAddressStep3').empty();   
    $('#benePhoneStep3').empty();  
    $('#receptionMethodStep3').empty();  
    $('#payeNameStep3').empty();  
    $('#payModeStep3').empty();  
    $('#payModeCardNoStep3').empty();  
    $('#beneNameStep2').empty(); 
	$('#youRSendStep2').text($('#youRSend').text());
    $('#sFeeStep2').text("00.00 USD"); 
    $('#totalAmtStep2').text("00.00 USD"); 
    $('#RecipientReceivesStep2').text("00.00 USD");
    $('#exchangeRateStep2').text("1 USD = 1.00 USD"); 
    $('.hideForPrmo').hide();
	$('#discountOnServiceFee').text("00.00 USD");
	$('#actualFee').text("00.00 USD");
	$('#totalAfertDiscountAmt').text("00.00 USD"); 
	$('#payerStep1').empty();
    $('#RecipientReceivesOpt').empty();
    $('#payerStep2').empty();
    $('#RecipientReceivesOptStep2').empty();
    $('#AdditionalField').empty();
    ALL_SER_FEE_OBJECT="";
}
 

function showTexasDisclaimerMessagePage(state)
{
	     var disclaimerMessage=document.getElementById("label.disclaimerMessage").value;
	     if(state == 'Texas')
	     {
	    	var texas=document.getElementById("label.texasCustomerNotice").value;
	    	disclaimerMessage=texas;
	     }else if(state == 'New York')
	     {
	    	 var newYork=document.getElementById("label.NewYorkDisclaimerMsg1").value;
	    	 disclaimerMessage=newYork;
	     }else if(state == 'California')
	     {
	    	 var california=document.getElementById("label.CaliforniaDisclaimerMsg1").value;
	    	 disclaimerMessage=california;
	     }	 
	     
	     loadingStart(); // start loading div
	     var tokenSecurity=$("#tokenSecurity").val();
	     var  uniseccodenc = $("#uniseccodenc").val();
	     var parameter="document="+state+"&tokenSecurity="+tokenSecurity+"&uniseccodenc="+uniseccodenc;
		 $.ajax({ 
		       url: "TexasDisclaimerMssage.action?request_locale="+locale,
		       headers: {"Content-type":"application/x-www-form-urlencoded","Accept":"text/html"},
		       type: 'POST',
		       data:parameter, 
		       success:function(jsonResponse) 
		       {   
		    	   loadingEnd();
		    	   if(null != jsonResponse && jsonResponse != 'undefied')
		    	   {
		    		 customMessageShowModel(jsonResponse,disclaimerMessage);  
		    	   }else
		    	   {
		    		 customErrorModal("Something wrong happend. Please try after some times"); 
		    	   }   
		    	    
		       },
		       error: function(jqXHR, exception) 
		        {  
		    	   
		    	 loadingEnd(); // End loading div
		    	 
		    	 sessionTimeOutGoBackLoginScreen(jqXHR);
	            }
		       
		      });  
 	
 }

function  fillAllDetailsOfSendStepTwo(payModeList,defaultRadioSelect,serviceFeeResponse)
{
     isfirstverified = false;
	 if(!(!payModeList) && payModeList!='undefied')
 {
	 payModeArr=payModeList;
	 var appenDiv="";  
	 bankAccSfee=serviceFeeResponse.bankAccountServiceFee;
	 CreditCardSfee=serviceFeeResponse.CreditCardServiceFee;
	 DebitCardSfee=serviceFeeResponse.DabitCardServiceFee; 
	
	 if(null==bankAccSfee || ''==bankAccSfee || 'undefined'==bankAccSfee)
	 {
		 bankAccSfee=sFee;
	 }
	 if(null==CreditCardSfee || ''==CreditCardSfee || 'undefined'==CreditCardSfee)
	 {
		 CreditCardSfee=sFee;
	 }
	 if(null==DebitCardSfee || ''==DebitCardSfee || 'undefined'==DebitCardSfee)
	 {
		 DebitCardSfee=sFee;
	 }
	 var bankAccCount=0; 
	 for(var i=0;i<payModeArr.length;i++)
	 {
		 var arrVal=payModeArr[i];
		 var j=i+1;
		 var radioButtonDiv="";
		 var deFaultSelect="";  
		 if(arrVal.paymentMode == 'Bank Account')
		 {  
			  if(arrVal.isVarifiedBankAc == 'YES')
		    {   
				  if(arrVal.id == defaultRadioSelect)
					  {
					    deFaultSelect="checked='checked'"; 
					    sendingMethodName=arrVal.paymentMode;
					  }
			   var deliveryMsg=document.getElementById("label.addNewBankTimingMsg").value; 
			   if(bankAccCount == 0)
			   { 
				   radioButtonDiv="<div class='sending-method__list-item clearfix'>"+
                   "<div class='sending-method__list-image'>"+
                   "<div class='sending-method__list-image--bestvalue text-center'>"+
                   "<p class='text-center small'>"+document.getElementById("label.bestValue").value+"</p>"+
                   "</div>"+
                   "</div>"+
                   "<div class='custom_radio'>"+
                   "<input type='radio' name='paymentModeId' id='bankaccount"+j+"' value='"+arrVal.id+"'  "+deFaultSelect+" onchange='updateServiceAndTotalOnChangePayMode()'>"+
                   "<label for='bankaccount"+j+"'><span class='radio'></span><strong class='dg-text bank-icon'><strong class='sending-method__list--title'>"+arrVal.cardNickName+" ["+padStar(arrVal.cardNo)+"]<br>"+
                   ""+document.getElementById("label.Fee").value+": <strong class='blue-text BankAccSfee' id='BankAccSfee'>$ "+bankAccSfee+"</strong></strong> <strong class='sending-method__list--desc'>"+document.getElementById("label.Delivery").value+": "+deliveryMsg+"</strong> </strong> </label>"+
                   "</div>"+
                   "</div>";   
			   }else
			   { 
				   radioButtonDiv="<div class='sending-method__list-item clearfix'>"+ 
                   "<div class='custom_radio'>"+
                   "<input type='radio' name='paymentModeId' id='bankaccount"+j+"' value='"+arrVal.id+"'  "+deFaultSelect+" onchange='updateServiceAndTotalOnChangePayMode()'>"+
                   "<label for='bankaccount"+j+"'><span class='radio'></span><strong class='dg-text bank-icon'><strong class='sending-method__list--title'>"+arrVal.cardNickName+" ["+padStar(arrVal.cardNo)+"]<br>"+
                   ""+document.getElementById("label.Fee").value+": <strong class='blue-text BankAccSfee' id='BankAccSfee'>$ "+bankAccSfee+"</strong></strong> <strong class='sending-method__list--desc'>"+document.getElementById("label.Delivery").value+": "+deliveryMsg+"</strong> </strong> </label>"+
                   "</div>"+
                   "</div>";   
				   
			   }
			   ++bankAccCount;  
		    }
			  
		 }else
		 {
			  if(arrVal.id == defaultRadioSelect)
			  {	
				 deFaultSelect="checked='checked'"; 
				 sendingMethodName=arrVal.paymentMode;
			  }
			  var cdDcSfee="";
			  var payModeFeeType="CCSFee";
			  if(arrVal.paymentMode == 'Credit Card')
			  {
				  cdDcSfee=CreditCardSfee;
				  payModeFeeType="CCSFee";
			  }else if(arrVal.paymentMode == 'Debit Card')
			  {
				  cdDcSfee=DebitCardSfee;
				  payModeFeeType="DCSFee";
			  }	  
			  
			   var deliveryMsg=document.getElementById("label.addNewCreditCardTimingMsg").value; 
			   radioButtonDiv="<div class='sending-method__list-item clearfix'>"+
               "<div class='sending-method__list-image'>"+
               "<div class='sending-method__list-image--fast-sending text-center'>"+
               "<p class='text-center small'>"+document.getElementById("label.fastest").value+" </p>"+
               "</div>"+
               "</div>"+
               "<div class='custom_radio'>"+
               "<input type='radio' name='paymentModeId' id='ccdccard"+j+"' value='"+arrVal.id+"'  "+deFaultSelect+" onchange='updateServiceAndTotalOnChangePayMode()'>"+
               "<label for='ccdccard"+j+"'><span class='radio'></span><strong class='dg-text card-icon'><strong class='sending-method__list--title'>"+arrVal.cardNickName+" ["+padStar(arrVal.cardNo)+"]<br>"+
               ""+document.getElementById("label.Fee").value+": <strong class='blue-text "+payModeFeeType+"' id='CCDCSfee'>$ "+cdDcSfee+"</strong></strong> <strong class='sending-method__list--desc'>"+document.getElementById("label.Delivery").value+": "+deliveryMsg+"</strong> </strong> </label>"+
               "</div>"+
               "</div>";  
			  
		 }
		
		 appenDiv=appenDiv+radioButtonDiv;
	 }  
	 document.getElementById("paymentModeDivId").innerHTML=appenDiv;
	 $('#beneNameStep2').text($('#beneName').text());
	 $('#youRSendStep2').text($('#youRSend').text());
     $('#sFeeStep2').text($('#sFee').text()); 
     $('#totalAmtStep2').text( $('#totalAmt').text()); 
     $('#RecipientReceivesStep2').text( $('#RecipientReceives').text());
     $('#exchangeRateStep2').text($('#exRate').text());  
     $('#payerStep2').text($('#payerStep1').text());
     $('#RecipientReceivesOptStep2').text($('#RecipientReceivesOpt').text());
     isfirstverified =true;   
 }else
	 {
	   var noSendingMethod=document.getElementById("label.noSendingMethod").value;
	   customErrorModal(noSendingMethod); 
	   return false;
	 } 
	 
}

function useAnotherPaymentMode()
{ 
	 showhideaddbeneficiary(7); 
}
 
function canceNewSendingMethod()
{
	showhideaddbeneficiary(10);
} 

function goBackAddNewSendingMethodScreen()
{
	if(addSendingMethodUrl=='payModeListScreen')
	{
		try
		{ 
			resetErrorsByContainerId("addNewBankAccount"); 
			$(".accountTypeRadioSpan").removeClass("errorRadio");
			hideErrorDiv();   
		}catch (e) {}
		try
		{ 
			resetErrorsByContainerId("addNewCreditCard");
			$(".cardTypeRadioSpan").removeClass("errorRadio"); 
			$(".ccdcTypeRadioSpan").removeClass("errorRadio");
			hideErrorDiv();   
	    }catch (e) {}
	    
		showhideaddbeneficiary(18);
		addSendingMethodUrl='';
		scrollTop(0);
	}
	else
	{
		try
		{ 
			resetErrorsByContainerId("addNewBankAccount"); 
			$(".accountTypeRadioSpan").removeClass("errorRadio");
			hideErrorDiv();   
		}catch (e) {}
		try
		{ 
			resetErrorsByContainerId("addNewCreditCard");
			$(".cardTypeRadioSpan").removeClass("errorRadio"); 
			$(".ccdcTypeRadioSpan").removeClass("errorRadio");
			hideErrorDiv();   
	    }catch (e) {}
	    
		showhideaddbeneficiary(11);
		scrollTop(0);
	}
	
} 
function addBankAccountView()
{
	addSendingMethodUrl="payModeListScreen";
	//checkAddBankAccountLimit();
	if(false==checkAddBankAccountLimit())
	{	
		addSendingMethodUrl='';
		//showhideaddbeneficiary(16);
	}
}

function addCCDCAccountView()
{
	CCDCFormValidateFunctionForAddSendingMethod(); 
	addSendingMethodUrl="payModeListScreen"; 
	var cardType=document.getElementById("label.CardType").value;
	fillCreditDebitCardDetails('',cardType,'');
	//showhideaddbeneficiary(17);  
}
function editNewBankAccount(paymentMode,id)
{
	editSendingMethodUrl="payModeListScreen"; 
	addBankAccDetailinForm(id); 
	scrollTop(0);
}
function goBackFromEditNewSendingMethodScreen()
{
	try{ resetErrorsByContainerId("editNewCreditCard"); hideErrorDiv();   }catch (e) {} 
	showhideaddsendingmethod(1);
	$("html, body").animate({ scrollTop: 0 }, "slow");
}
function editNewCCAndDC(paymentMode,id)
{
	//alert(paymentMode+" "+id);
	try{ resetErrorsByContainerId("editNewCreditCard"); hideErrorDiv();   }catch (e) {}
	addCCAndDcValues(id);
	showhideaddsendingmethod(2);
}
function addCCAndDcValues(id)
{
	var tokenSecurity=$("#tokenSecurity").val();
	var  uniseccodenc = $("#uniseccodenc").val();

	 var parameter="rpmIdForEdit="+id+"&tokenSecurity="+tokenSecurity+"&uniseccodenc="+uniseccodenc;
	 $.ajax({ 
	       url: "editPaymentMode.action?request_locale="+locale,
	       headers: {"Content-type":"application/x-www-form-urlencoded"},
	       type: 'POST',
	       data:parameter, 
	       dataType: 'json',
	       success:function(jsonResponse) 
	       {   
	    	   loadingEnd();
	    	   if(null != jsonResponse && jsonResponse != 'undefied')
	    	   {
	    		      var errorMsg=jsonResponse.errorMsgString;
			    	  if(null!=errorMsg && errorMsg!='' && errorMsg!='undefied')
					  {
			    		  customErrorModal(errorMsg); 
			    		  return false;
					  }else
					  {
						// customErrorModal(jsonResponse);
						  var responceObj = JSON.parse(jsonResponse.ccdcJsonResponse); 
						  var cardNumber=responceObj.cardNumber;
						  var address1=responceObj.address1;
						  var address2=responceObj.address2;
						  var city=responceObj.city;
						  var state=responceObj.state;
						  var remitterCountry=responceObj.remitterCountry;
						  var remitterZipcode=responceObj.remitterZipcode;
						  var monthList=responceObj.monthList;
						  var yearList=responceObj.yearList;
						  var cardType=responceObj.cardType;
						  var nameOnCard=responceObj.nameOnCard;
						  var stateLists=responceObj.stateList; 
						  
						  if(null ==address2 || address2 == '' && address2 =='undefied')
			    		   {
			    			   address2="";
			    		   }
			    		  
			    		   var monthListOption="<option value=''>MM</option> ";
			    		   for (var i = 0; i < monthList.length; i++) 
					       {
			    			  monthListOption += "<option value='"+monthList[i]+"'>" +monthList[i]+ "</option>";  
					       }  	   
			    		   
			    		   var yearListOption="<option value=''>"+ document.getElementById("label.YYYY").value+"</option> ";
			    		   for (var j = 0; j < yearList.length; j++) 
					       {
			    			   yearListOption += "<option value='"+yearList[j]+"'>" +yearList[j]+ "</option>";  
					       }  
			    		   
			    		   var stateOptionList="";
					    	  for (var k = 0; k < stateLists.length; k++) 
					       {
					    		  if(state == stateLists[k].KEY) 
						   		   {
					    			  stateOptionList += "<option value='"+stateLists[k].KEY+"' selected>" +stateLists[k].VALUE+ "</option>";      
						   		   }else
						   		   {
						   			  stateOptionList += "<option value='"+stateLists[k].KEY+"'>" +stateLists[k].VALUE+ "</option>";     
						   		   }	 
					       }
			    		  // alert(nameOnCard);
			    		   document.getElementById("nameOnCard1").value=nameOnCard;
			    		   document.getElementById("CCDCNumber1").value=cardNumber;
			    		   document.getElementById("CCDCAddress11").value=address1;
						  document.getElementById("CCDCAddress12").value=address2;
						  document.getElementById("CCDCCity1").value=city;
						  //document.getElementById("CCDCCState1").value=statesList;
						  //document.getElementById("CCDCCCountry1").value=remitterCountry;
						  document.getElementById("billingZipCode1").value=remitterZipcode;
						  document.getElementById("month1").innerHTML = monthListOption;
					      document.getElementById("year1").innerHTML=yearListOption;
					      document.getElementById("CCDCCState1").innerHTML=stateOptionList;
					      document.getElementById("rpmIdForEdit").value=id;
					       
					       if(cardType=='Visa')
					    	{
					    	   document.getElementById("visa1").checked = true;
					    	}
					       else
					    	{
					    	   document.getElementById("masterCard1").checked = true;
					    	}
					       
						  
					  }
	    		   
	    	   }else
	    	   {
	    		 customErrorModal("Something wrong happend. Please try after some times"); 
	    	   }   
	    	    
	       },
	       error: function(jqXHR, exception) 
	        {  
	    	   
	    	 loadingEnd(); // End loading div 
	    	 sessionTimeOutGoBackLoginScreen(jqXHR);
          }
	       
	      }); 
}



function addBankAccDetailinForm(id)
{
	
	 loadingStart();
	 var tokenSecurity=$("#tokenSecurity").val();
	 var  uniseccodenc = $("#uniseccodenc").val(); 
	 var parameter="sendingMethodId="+id+"&tokenSecurity="+tokenSecurity+"&uniseccodenc="+uniseccodenc;
	 $.ajax({ 
	       url: "editbankAccPaymentMode.action?request_locale="+locale, 
	       headers: {"Content-type":"application/x-www-form-urlencoded"},
	       type: 'POST',
	       data:parameter, 
	       dataType: 'json',
	       success:function(jsonResponse) 
	       {   
	    	   
	    	   if(null != jsonResponse && jsonResponse != 'undefied')
	    	   {
	    		      var errorMsg=jsonResponse.errorMsgString;
			    	  if(null!=errorMsg && errorMsg!='' && errorMsg!='undefied')
					  {
			    		  customErrorModal(errorMsg); 
			    		  return false;
					  }else
					  {
						// customErrorModal(jsonResponse);
						  var responceObj = JSON.parse(jsonResponse.bankJsonResponse); 
						  var plaidFlag=responceObj.addedByPlaid;
						  if(null!=plaidFlag && ''!=plaidFlag && 'undefined'!=plaidFlag && plaidFlag=='YES')
						  {
							  var pmid=responceObj.pmid;
							  openUrlSecurity("plaidUserIdScreen.action?pmid="+pmid);
						  }else
						  {
							  loadingEnd();
							  var bankName=responceObj.bankName;
							  var accType=responceObj.accType;
							  var routingNumber=responceObj.routingNumber;
							  var accNumber=responceObj.accNumber;
							  var cnfAccNumber=responceObj.cnfAccNumber;
							  var accNickName=responceObj.accNickName;  
				    		  document.getElementById("bankName1").value=bankName; 
				    		  document.getElementById("routingNumber1").value=routingNumber;
							  document.getElementById("bankAccountNumber1").value=accNumber;
							  document.getElementById("bankconfirmAccountNumber1").value=cnfAccNumber;
							  document.getElementById("accountNickName1").value=accNickName; 
							  document.getElementById("sendingMethodId").value=id;  
						       if(accType=='Saving')
						    	{
						    	   document.getElementById("radio03").checked = true;
						    	}
						       else
						    	{
						    	   document.getElementById("radio04").checked = true;
						    	} 
						       
						       showhideaddsendingmethod(0);
						  }
						  
					     }
	    		   
	    	   }else
	    	   {
	    		 customErrorModal("Something wrong happend. Please try after some times"); 
	    	   }   
	    	    
	       },
	       error: function(jqXHR, exception) 
	        {  
	    	   
	    	 loadingEnd(); // End loading div 
	    	 sessionTimeOutGoBackLoginScreen(jqXHR);
          }
	       
	      });
}

function updateTransferSummaryAfterUpdateserviceFeeAndExrate()
{ 
    var selectedBeneId =document.getElementById("beneId")[document.getElementById("beneId").selectedIndex].value;
	var sAmount=document.getElementById("sendingAmount").value;   
	$('#confirmModal').modal('hide');
	var tokenSecurity=$("#tokenSecurity").val();
	var  uniseccodenc = $("#uniseccodenc").val();
	var limitCheck = document.getElementById("limitCheck").value;
	$.ajax({
	        type: 'POST',
            url: 'fetchTransFerSummary.action?request_locale='+locale,
            headers: {"Content-type":"application/x-www-form-urlencoded"},
            data : "beneficiaryId="+selectedBeneId+"&startAmt="+parseFloat(sAmount).toFixed(2)+"&paymentModeName="+sendingMethodName+"&tokenSecurity="+tokenSecurity+"&uniseccodenc="+uniseccodenc,
            dataType: 'json',
            success: function(jsonResponse)
            {
            	  var errorMsg=jsonResponse.errorMessage; 
	   	    	  if(null!=errorMsg && errorMsg!='')
	   	    		  {
	   	    		      customErrorModal(errorMsg); 
	   	    		  }else if(!(!jsonResponse.serviceFee) && "undefined" != jsonResponse.serviceFee)
                      { 
			             var cur='USD';
			             sFee=jsonResponse.serviceFee;
			             sFeeBeforHitCore=sFee;
			             $('#sFee').text(jsonResponse.serviceFee+" "+cur); 
			             $('#totalAmt').text(jsonResponse.yourTotal+" "+cur);
			             $('#sFeeStep2').text(jsonResponse.serviceFee+" "+cur); 
		        	     $('#totalAmtStep2').text(jsonResponse.yourTotal+" "+cur); 
		        	     $('.exchangeRate').text("1 "+cur+" = "+jsonResponse.exRate+" "+jsonResponse.payerCurrencyId);  
			             $('#exchangeRateStep2').text("1 "+cur+" = "+jsonResponse.exRate+" "+jsonResponse.payerCurrencyId);
			             $('#RecipientReceives').text(jsonResponse.receiveAmt+" "+jsonResponse.payerCurrencyId);
			             $('#RecipientReceivesStep2').text(jsonResponse.receiveAmt+" "+jsonResponse.payerCurrencyId);
			             $('#exchangeCurrency').text(jsonResponse.payerCurrencyId); 
			             document.getElementById("recipientReceivestextId").value=jsonResponse.receiveAmt;
			             if(sendingMethodName == 'Bank Account')
			             {
			            	 $(".BankAccSfee").text("$ "+jsonResponse.serviceFee);
			            	 bankAccSfee=jsonResponse.serviceFee;
			             }else if(sendingMethodName == 'Credit Card')
			             {
			            	 $(".CCSFee").text("$ "+jsonResponse.serviceFee);
			            	 CreditCardSfee=jsonResponse.serviceFee;
			             }else if(sendingMethodName == 'Debit Card')
			             {
			            	 $(".DCSFee").text("$ "+jsonResponse.serviceFee);
			            	 DebitCardSfee=jsonResponse.serviceFee;
			             }
                      } 
	   	    	  
	   	    	      updateServiceAndExrateObjAfterUpdateSfee(selectedBeneId,limitCheck);
            },error: function(jqXHR, exception) 
  	       {  
      	 	   	 loadingEnd(); // End loading div
      	 	     sessionTimeOutGoBackLoginScreen(jqXHR);
      	 	     
      	          } 
            
	
	});    
}

function updateServiceAndTotalOnChangePayMode()
{
	   var payModeSelected=($('input[name=paymentModeId]:checked')); 
	   var sendingAmt=$('#sendingAmount').val();
	   var payModeVal="";
	   defaultPaymodeId="";
	   if( sFee!=bankAccSfee || sFee!=DebitCardSfee || sFee!=CreditCardSfee )
	  { 	   
		  
		   if (payModeSelected.length > 0) 
		    {
			   payModeVal = payModeSelected.val();
			} 
		   
		   for(var i = 0; i < payModeArr.length; i++ ) 
		   {
		        if(payModeVal == payModeArr[i].id) 
		        {   
		        	sendingMethodName=payModeArr[i].paymentMode;
		        	defaultRadioSelect=payModeArr[i].id;
		        	if(sendingMethodName == 'Bank Account')
		        	{ 
		        		 var total=(parseFloat(sendingAmt)+parseFloat(bankAccSfee));
		        		 defaultPaymodeId="004";
		        	     $('#sFee').text(bankAccSfee+" USD"); 
		        	     $('#totalAmt').text(total.toFixed(2)+" USD");
		        	     $('#sFeeStep2').text(bankAccSfee+" USD"); 
		        	     $('#totalAmtStep2').text(total.toFixed(2)+" USD");  
		        	     changeServiceFeeInSession(bankAccSfee);
		        	     
		        	}else if(sendingMethodName == 'Credit Card')
		        	{ 
		        		 var total=(parseFloat(sendingAmt)+parseFloat(CreditCardSfee));
		        		 defaultPaymodeId="002";
		        		 $('#sFee').text(CreditCardSfee+" USD"); 
		        	     $('#totalAmt').text(total.toFixed(2)+" USD");
		        	     $('#sFeeStep2').text(CreditCardSfee+" USD"); 
		        	     $('#totalAmtStep2').text(total.toFixed(2)+" USD");  
		        	     changeServiceFeeInSession(CreditCardSfee);
		        	     
		        	}else if(sendingMethodName == 'Debit Card')
		        	{ 
		        		 var total=(parseFloat(sendingAmt)+parseFloat(DebitCardSfee));
		        		 defaultPaymodeId="003";
		        		 $('#sFee').text(DebitCardSfee+" USD"); 
		        	     $('#totalAmt').text(total.toFixed(2)+" USD");
		        	     $('#sFeeStep2').text(DebitCardSfee+" USD"); 
		        	     $('#totalAmtStep2').text(total.toFixed(2)+" USD"); 
		        	     changeServiceFeeInSession(DebitCardSfee);
		        	}	
		        	 
		        	break;
		        } 
		   }
		   
	   }  
}

function colardoMessageShowPopUp()
{
	var Colorado=document.getElementById("label.ColoradoDisclaimerMsg1").value;
	var title=Colorado;
	var coloradoPDF='<iframe src="/Uniteller/Terms_Condition_Disclaimer/colorado_customer_notice.pdf" style="zoom:0.60" width="99.6%" height="500" frameborder="0"></iframe>';
	customMessageShowModel(coloradoPDF,title); 
	
}
function verifyNewBankAccount()
{
	showhideaddsendingmethod(3);
}
 
function directSendAgain(jsaonRes,feeNRateChangeFlag)
{ 
	 isfirstverified=true;
	 issecondverified=true; 
	 var responseFor3Step=JSON.parse(jsaonRes.replace(/&quot;/g,'"'));  
	 gotoSendStep3(responseFor3Step,feeNRateChangeFlag);   
	 $("#beneficiarybodytitle .steps-title").removeClass("active"); 
	 $("#beneficiarybodytitle .steps-title").addClass("disable");
	 $("#confirmationtitle .steps-title").addClass("active");
	 isfirstverified=false;
	 issecondverified=false; 
	 defaultFirst=false;
 
}
function reSetPromotionByDefaultVal()
{
	 $('.hideForPrmo').hide();
	 $('#discountOnServiceFee').text("00.00 USD");
	 $('#actualFee').text("00.00 USD");
	 $('#totalAfertDiscountAmt').text("00.00 USD"); 
	 $('#applyPromoCode').val('');
	 $("#promoCodeButton").prop('disabled', false);
	 $('#applyPromoCode').removeAttr("readonly");;
}

function changeServiceFeeInSession(serviceFee)
{
	
	sFeeBeforHitCore=serviceFee;
	/*var tokenSecurity=$("#tokenSecurity").val();
	var  uniseccodenc = $("#uniseccodenc").val();
	     
		$.ajax({
		        type: 'POST',
	            url: 'updateSfeeInSession.action?request_locale='+locale,
	            headers: {"Content-type":"application/x-www-form-urlencoded"},
	            data : "serviceFee="+serviceFee+"&tokenSecurity="+tokenSecurity+"&uniseccodenc="+uniseccodenc,
	            dataType: 'json',
	            success: function(jsonResponse)
	            {
	            	   
	            },});    */
}

function gotoConfirmationTransactionScreen()
{
	
	
	var tokenSecurity=$("#tokenSecurity").val();
	$.ajax({ 
		
	       url: "sendMoneyConfirm.action?request_locale="+locale, 
	       headers: {"Content-type":"application/x-www-form-urlencoded"},
	       type: 'POST', 
	       data:"tokenSecurity="+tokenSecurity,
	       dataType: 'json',
	       success:function(jsonResponse) 
	       {  
	    	   try{hj('trigger','send_step_3');}catch(e){console.log("error in hj('trigger','send_step_3')");}   
	    	
	    	   if(null != jsonResponse && jsonResponse != 'undefied')
	    	   {
	    		   $("#tokenSecurity").val(jsonResponse.tokenSecurity);
	    		      var errorResponseOnConfirm=jsonResponse.errorResponseOnConfirm;
			    	  if(null!=errorResponseOnConfirm && errorResponseOnConfirm!='' && errorResponseOnConfirm!='undefied')
					  {
			    		  if(errorResponseOnConfirm == 'cardError')
			    		  {
			    			  
			    			  openUrlSecurity("fundTransferInit.action");
			    			  
			    		  }else if(errorResponseOnConfirm == 'txMightCreated')
			    		  {
			    			  openUrlSecurity("homeforward.action?txmightCreated=1");
			    			  
			    		  }else if(errorResponseOnConfirm == 'duplicateTx')
			    		  {
			    			  openUrlSecurity("fundTransferInit.action?fromSendMoney=doNotClearSession");
			    			  
			    		  }else if(errorResponseOnConfirm == 'promocodefailure')
			    		  {
			    			  openUrlSecurity("homeforward.action?promoCodeError=1");
			    			  
			    		  }else if(errorResponseOnConfirm == 'fraudUser')
			    		  {
			    			  window.location.href="LogoutAction.action";
			    			  
			    		  }else
			    		  {
			    			  openUrlSecurity("homeforward.action?systemError=1");
			    		  }	   
			    		  
			    		  return false;
					  }else
					  {
						  loadingEnd();
						  var confirmReceiptresponceObj = JSON.parse(jsonResponse.confirmBillJsonResponse); 
						  fillConfirmReceiptScreen(confirmReceiptresponceObj); 
					  }
	    		   
	    	   }else
	    	   {
	    		   loadingEnd();
	    		   customErrorModal("Something wrong happend. Please try after some times"); 
	    	   }   
	    	    
	       },
	       error: function(jqXHR, exception) 
	        {  
	    	   
	    	 loadingEnd(); // End loading div 
	    	 sessionTimeOutGoBackLoginScreen(jqXHR);
            }
	       
	      }); 
}

function fillConfirmReceiptScreen(confirmReceiptresponceObj)
{
	
	 if(null==confirmReceiptresponceObj || ''==confirmReceiptresponceObj || 'undefined'==confirmReceiptresponceObj)
	 {
		 customErrorModal('Something wrong happend. Please try after some times');
	 }else
     { 
	  
	  var txNumber=confirmReceiptresponceObj.txNumber; 
	  var amountTobeReceived=confirmReceiptresponceObj.amountTobeReceived;
	  var txTransferAmt=confirmReceiptresponceObj.txTransferAmt;
	  var holdTxMessage=confirmReceiptresponceObj.holdTxMessage;
	  var beneReceptionMethod=confirmReceiptresponceObj.beneReceptionMethod;		
	  var txReleaseDate=confirmReceiptresponceObj.txReleaseDate;
	  var txExchangeRate=confirmReceiptresponceObj.txExchangeRate;
	  var sendingMethod=confirmReceiptresponceObj.sendingMethod;
	  var reprintLabel=confirmReceiptresponceObj.reprintLabel; 
	  var remitterFullName=confirmReceiptresponceObj.remitterFullName;
	  var txCurrency=confirmReceiptresponceObj.txCurrency;
	  var txServiceFee=confirmReceiptresponceObj.txServiceFee;
	  var txExchangeCurr=confirmReceiptresponceObj.txExchangeCurr;
	  var txStatus=confirmReceiptresponceObj.txStatus;
	  var sendingMethodType=confirmReceiptresponceObj.sendingMethodType;
	  var beneFullName=confirmReceiptresponceObj.beneFullName;
	  var txConfirmDate=confirmReceiptresponceObj.txConfirmDate;
	  var txToalAmount=confirmReceiptresponceObj.txToalAmount;
	  var cardNumber=confirmReceiptresponceObj.cardNumber;
	  var benePayerName= confirmReceiptresponceObj.benePayerName;
	  var tranStatus= confirmReceiptresponceObj.txStatus; 
	  var txActualSFee= confirmReceiptresponceObj.txActualSFee;
	  var txDiscountOnSfee= confirmReceiptresponceObj.txDiscountOnSfee;
	  var totalTxWithDiscount= confirmReceiptresponceObj.totalTxWithDiscount;
	  var beneAccountNo='';
	  var beneId=confirmReceiptresponceObj.beneId;
	    
			
	  var url = "/jsps/firstBeneficiary.action?request_locale=en&beneId="+beneId;
	  document.getElementById("alertsetupId").onclick = function () { openUrlSecurity(url); };
		  
	  
	  if(null!=beneReceptionMethod && ''!=beneReceptionMethod && beneReceptionMethod == 'Account Credit')
	  {
		  beneAccountNo=confirmReceiptresponceObj.beneAccountNo;
		  beneReceptionMethod=document.getElementById("AccountCredit").value;
		  $("#beneAccountNo").text(beneAccountNo);
		  $(".beneAccountCreditTr").show();
		  $(".hideforaccountcredit").hide();
		  
	  }else
	  {
		  $(".beneAccountCreditTr").hide();
		  beneReceptionMethod=document.getElementById("CashPickup").value;
	  }	  
	  
	  if(null!=sendingMethod && ''!=sendingMethod && sendingMethod == 'Credit Card')
	  { 
		  var val=document.getElementById("CreditCard").value;
		  $("#sendingMethodTypeId").text(val);
		  $("#beneCCDCType").text(sendingMethodType);
		  $("#beneCCDCNumber").text(cardNumber);
		  $(".creditDebitCardTrId").show();
		  
	  }else if(null!=sendingMethod && ''!=sendingMethod && sendingMethod == 'Debit Card')
	  {
		  var val=document.getElementById("DebitCard").value;
		  $("#sendingMethodTypeId").text(val);
		  $("#beneCCDCType").text(sendingMethodType);
		  $("#beneCCDCNumber").text(cardNumber);
		  $(".creditDebitCardTrId").show();
		  
	  }else if(null!=sendingMethod && ''!=sendingMethod && sendingMethod == 'Bank Account')
	  {
		  var val=document.getElementById("BankAccount").value;
		  $("#sendingMethodTypeId").text(val);
		  $("#beneBankName").text(sendingMethodType);
		  $("#beneBankAccNo").text(cardNumber);
		  $(".bankAccountTrId").show();
		  
	  }else if(null!=sendingMethod && ''!=sendingMethod && sendingMethod == 'ULink')
	  {
		  var val=document.getElementById("UlinkCard").value;
		  $("#sendingMethodTypeId").text(val);
		  $("#beneCCDCType").text(sendingMethodType);
		  $("#beneCCDCNumber").text(cardNumber);
		  $(".creditDebitCardTrId").show();
	  }	  
	  if((null!=txActualSFee && ''!=txActualSFee && 'undefined'!=txActualSFee) && (null!=txDiscountOnSfee && ''!=txDiscountOnSfee && 'undefined'!=txDiscountOnSfee))
	  {
		  $("#afterConfirmDiscountOnSFee").text(txDiscountOnSfee+" "+txCurrency);
		  $("#afterConfirmDiscountedSFee").text(txServiceFee);
		  $("#afterConfirmDiscountTotalAmt").text(totalTxWithDiscount+" "+txCurrency);
		  $("#afterConfirmTxSfeeId").text(txActualSFee+" "+txCurrency);
		  $(".ConfirmReceiptAfterDiscount").show();
	  }else
	  {
		  $("#afterConfirmTxSfeeId").text(txServiceFee);
		  $(".ConfirmReceiptAfterDiscount").hide();
	  }	  
	  
	  $("#txNumber").text(txNumber);
	  $("#afterConfirmAmtReceived").text(amountTobeReceived);
	  $("#transferAmontId").text(addCommaSep(txTransferAmt,true));
	  //$("#holdMsgOnConfirmReceipt").text(holdTxMessage);
	  $("#receptionMethOnConfirmReceipt").text(beneReceptionMethod);
	  $("#pNameOnConfirmReceipt").text(benePayerName);
	  $("#afterConfirmExrate").text(txExchangeRate);
	  $("#remiNameOnConfirmReceipt").text(remitterFullName);
	  //$("#afterConfirmTxSfeeId").text(txServiceFee);
	  $("#beneNameOnConfirmReceipt").text(beneFullName);
	  $("#txPayableDate").text(txReleaseDate);
	  $("#afterConfirmTotalAmt").text(addCommaSep(txToalAmount,true)); 
	  $("#transStatus").text(tranStatus);
	  //Staty Connected Items Ankur
	  $("#benePhoneForSC").val(confirmReceiptresponceObj.benePhone);
	  $("#beneEmailForSC").val(confirmReceiptresponceObj.beneEmail);
	  $("#SCPopUpBeneCountryImage").attr('src', confirmReceiptresponceObj.beneCountryImageSrc);
	  $("#SCCountryISDCode").text(confirmReceiptresponceObj.SCCountryISDCode);
	  
	  //Staty Connected Items Ankur
	  isfirstverified=false;
	  issecondverified=false; 
	  defaultFirst=false;
	  showhideConfirmReceipt(0);  
	  var SMSAlertsSwitchBene=confirmReceiptresponceObj.beneAlertNotificationSwitch;
	 // alert(SMSAlertsSwitchBene);
	 if(!(!SMSAlertsSwitchBene) && "ON"==(SMSAlertsSwitchBene.toUpperCase()))
		  {
		// alert("Opening popup.");
	          setTimeout(function(){  $("#myModalStep2").modal(); }, 3000);
	
		  }
     }

}

function getTextCustom(val){
	var result = val;
	if("Credit Card"== val){
		try{result = document.getElementById("CreditCard").value;}catch (e) {}
		return result;
	}else if("Debit Card"==val){
		try{result = document.getElementById("DebitCard").value;}catch (e) {}
		return result;
		
	}else if("Bank Account" == val){
		try{result = document.getElementById("BankAccount").value;}catch (e) {}
		return result;
		
	}else if("uLink Card"==val){
		try{result = document.getElementById("UlinkCard").value;}catch (e) {}
		return result;
		
	}else{
		return result;
	}
	
	
}
//for Additional Filed-: Start here
function checkAdditionalField(beneficiaryID){
	 
	var tokenSecurity=$("#tokenSecurity").val();
	var  uniseccodenc = $("#uniseccodenc").val();

	    var appenChild=""; 
	    $.ajax({
        type: 'POST',
        url: 'fetchTxAdditionalField.action',
        headers: {"Content-type":"application/x-www-form-urlencoded"},
        data : "beneficiaryId="+beneficiaryID+"&tokenSecurity="+tokenSecurity+"&uniseccodenc="+uniseccodenc,	       
        dataType: 'json',
        success: function(jsonResponse)
        {
            var filledArray=jsonResponse.customFieldDetails;
            var istrue=jsonResponse.isTxNotAllow;
            if(null != istrue && istrue=='true' && istrue !='undefined'){
            	customInfoModal(jsonResponse.infoMsgString);
            	return false;
            }else
            { 
            	if(filledArray.length > 0){
            		moreThanOneAdditionalField();
            	}
            	else{
            		$('#AdditionalField').empty();
            	}

            }
           
	        
        },error: function(jqXHR, exception) 
	       {  
  	 	   	 loadingEnd(); // End loading div
  	 	     sessionTimeOutGoBackLoginScreen(jqXHR);
  	 	     
  	          }     
     	
     	});     
   
}

function moreThanOneAdditionalField()
{
	var tokenSecurity=$("#tokenSecurity").val();
	var  uniseccodenc = $("#uniseccodenc").val();
	var selectedBeneId =document.getElementById("beneId")[document.getElementById("beneId").selectedIndex].value;
	 loadingStart(); // start loading div
	 $.ajax({ 
	       url: "showAdditionFieldOnPopUp.action?beneficiaryId="+selectedBeneId+"&tokenSecurity="+tokenSecurity+"&uniseccodenc="+uniseccodenc, 
	       headers: {"Content-type":"application/x-www-form-urlencoded"},
	       type: 'POST', 
	       success:function(jsonResponse) 
	       {   
	    	   loadingEnd(); 
	    	   if(null != jsonResponse && jsonResponse != 'undefied')
	    	   {
	    		   customAdditionalFieldShowModel(jsonResponse,"Additional fields");  
	    	   }else
	    	   {
	    		 customErrorModal("Something wrong happend. Please try after some times"); 
	    	   }   
	       },
	       error: function(jqXHR, exception) 
	        {  
	    	   
	    	 loadingEnd(); // End loading div
	    	 sessionTimeOutGoBackLoginScreen(jqXHR);  
            }
	       
	      });   
}
function validateFieldDate()
{  
	   var isAlert = false;
	   var isInvalidDate=false;
		$('#additionalFieldDiv').find('input ').each(function(){
			if(!$(this).val())
				{
				isAlert = true; 
				}
			if($(this).attr("isDateType")){
				if(!validCalender($(this).attr("id"))){
					isInvalidDate = true; 
				}
			}
			
		});
		$('#additionalFieldDiv').find('select ').each(function(){
			if(!$(this).val()) isAlert = true; 
		}); 
		 
		if(isAlert) 
			{
			  $('#fieldError').text("Please fill all mandatory field.");
			  $('#errorMsgForAdditional').show();
			  return false;
			}
		else if(isInvalidDate)
		{
			  $('#fieldError').text("Please provide valid date format.");
			  $('#errorMsgForAdditional').show();
			  return false;
			
	     }
		else
			 {
				$('#fieldError').empty();
				var formElements=document.getElementById("additionFieldForm").elements;    
				var ParametersMap="";
				var beneId=document.getElementById("beneficiaryId").value; 
				var count=1;
				for (var i=0; i<formElements.length; i++)
					{
				    if (formElements[i].type!="submit" && formElements[i].type!="button") 
				    	{
				    	     var key=formElements[i].name;
				    	     var value=formElements[i].value;
					    	 if(count!=1)
				        	 {
			                	
			                	ParametersMap=ParametersMap+"&"+key+"="+value;
				        	 }
			                 else
			                 {
			                	ParametersMap=ParametersMap+key+"="+value;
			                 }
				         
				    	}
				        count++;
					} 
				
				var tokenSecurity=$("#tokenSecurity").val();
				var  uniseccodenc = $("#uniseccodenc").val();

				  ParametersMap=ParametersMap+"&beneId="+beneId+"&tokenSecurity="+tokenSecurity+"&uniseccodenc="+uniseccodenc;  
				  $.ajax({
					        type : 'POST',
					        url  : 'saveAdditionalFieldValue.action',
					        headers: {"Content-type":"application/x-www-form-urlencoded"},
					        data : ParametersMap,	       
					        dataType: 'json',
					        success: function(jsonResponse)
					        {       
					        	var msg=document.getElementById("label.AdditionalFieldSaveMsg").value; 
					        	var n=document.getElementById('label.editAdditionalFieldMsg').value;
					        	var butt="<button class='btn btn-link add-beneficiary--btn' onclick='showAllAdditionalFieldForEdit();'><span class='add-beneficiary--icon'>"+n+"</span></button>";
					        	$('#fieldError').text(msg);	 
					        	$('#fieldError').css('color', 'green');
					    		document.getElementById("AdditionalField").innerHTML=butt;
					        	$('#additionalFieldModel').modal('hide');
					        	
					     },error: function(jqXHR, exception) 
				 	       {  
				   	 	   	 loadingEnd(); // End loading div
				   	 	     sessionTimeOutGoBackLoginScreen(jqXHR);
				   	 	     
				   	          }   
				     	});  
			 } 
}
function showAllAdditionalFieldForEdit()
{
	var selectedBeneId =document.getElementById("beneId")[document.getElementById("beneId").selectedIndex].value;
	 loadingStart(); // start loading div
	 var tokenSecurity=$("#tokenSecurity").val();
	 var  uniseccodenc = $("#uniseccodenc").val();
	 $.ajax({ 
	       url: "getAllAdditionalFieldForEdit.action?beneficiaryId="+selectedBeneId+"&tokenSecurity="+tokenSecurity+"&uniseccodenc="+uniseccodenc,
	       headers: {"Content-type":"application/x-www-form-urlencoded"},
	       type: 'POST', 
	       success:function(jsonResponse) 
	       {   
	    	   loadingEnd(); 
	    	   if(null != jsonResponse && jsonResponse != 'undefied')
	    	   {
	    		   customAdditionalFieldShowModel(jsonResponse,"Additional fields");  
	    	   }else
	    	   {
	    		 customErrorModal("Something wrong happend. Please try after some times"); 
	    	   }   
	       },
	       error: function(jqXHR, exception) 
	        {  
	    	   
	    	 loadingEnd(); // End loading div
	    	 sessionTimeOutGoBackLoginScreen(jqXHR);  
            }
	       
	      });   
}
//  End here

function getTxSummeryByAmt(sendingAmount)
{
	var sendAmt=sendingAmount;
	var ser_fee_obj_len=ALL_SER_FEE_OBJECT.length;
	var fee="";
	var count=0;
	for(var i=0;i<ser_fee_obj_len;i++)
	{
		var start_amt=ALL_SER_FEE_OBJECT[i].startAmt;
		var end_amt=ALL_SER_FEE_OBJECT[i].endAmt;
		
		if(sendAmt>=start_amt && sendAmt<=end_amt)
		{
			
			if(count==0)
			{
				 fee=ALL_SER_FEE_OBJECT[i].fee;
				 defaultPaymodeId=ALL_SER_FEE_OBJECT[i].paymentMode;
			}
			else if(fee>ALL_SER_FEE_OBJECT[i].fee)
			 {
				 fee=ALL_SER_FEE_OBJECT[i].fee;
				 defaultPaymodeId=ALL_SER_FEE_OBJECT[i].paymentMode;
			 }
			count++; 
		}
	}
    return fee;
}

// for adding commas in number format like 1,258.43, 15,000.00
// argument is any number and true/False
function addCommaSep(somenum,usa){    
	  var dec = String(somenum).split(/[.,]/)
	     ,sep = usa ? ',' : '.'
	     ,decsep = usa ? '.' : ',';
	  return dec[0]
	         .split('')
	         .reverse()
	         .reduce(function(prev,now,i){
	                   return i%3 === 0 ? prev+sep+now : prev+now;}
	                )
	         .split('')
	         .reverse()
	         .join('') +
	         (dec[1] ? decsep+dec[1] :'')
	  ;
	}

function removeCommaSeperatorFromAmount(str)
{  
	    while (str.search(",") >= 0) 
	    {
	        str = (str + "").replace(',', '');
	    }
	    
	    return str;  
}

function updateServiceAndExrateObjAfterUpdateSfee(selectedBeneId,limitCheck)
{
  
	if((null!=selectedBeneId && ''!=selectedBeneId))
	{ 
		var tokenSecurity=$("#tokenSecurity").val();
		var  uniseccodenc = $("#uniseccodenc").val();
		   $.ajax({ 
   	       url: "fetchExchangeRate.action?request_locale="+locale, 
   	       headers: {"Content-type":"application/x-www-form-urlencoded"},
   	       type: 'POST',
   	       data:"beneficiaryId="+selectedBeneId+"&tokenSecurity="+tokenSecurity+"&uniseccodenc="+uniseccodenc+"&dailyLimit="+limitCheck,
   	       dataType: 'json',
   	       success:function(jsonresponse) 
   	       { 
   	    	  loadingEnd();
   	    	  $("#tokenSecurity").val(jsonresponse.tokenSecurity);  
   	    	  var errorMsg=jsonresponse.errorMessage; 
   	    	  if(null!=errorMsg && errorMsg!='')
   	          {
   	    		      customErrorModal(errorMsg); 
   	    	  }else
   	    	  { 
     	    	      var ALL_SER_FEE=jsonresponse.exRateNadSfee.SER_FEE;
     	    	      ALL_SER_FEE_OBJECT=ALL_SER_FEE; 
   	    	  } 
   	    	  
   	       },error: function(jqXHR, exception) 
 	       {  
     	 	   	 loadingEnd(); // End loading div
     	 	     sessionTimeOutGoBackLoginScreen(jqXHR);
     	 	     
     	          }
	       
   	      }); 

     }else
     {
    	 loadingEnd();
    	 $('#exchangeCurrency').text(''); 
    	 document.getElementById("exchangeRate").value="";
    	 $('#payerStep1').empty(); 
		 $('#RecipientReceivesOpt').empty(); 
     }	 
	
}