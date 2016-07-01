
$MegaMotionScenesWindowID = 159;
$MegaMotionSequenceWindowID = 395;

$mmAddProjectWindowID = 241;
$mmAddSceneWindowID = 246;
$mmAddSceneShapeWindowID = 247;
$mmAddShapeGroupWindowID = 333;
$mmAddOpenSteerWindowID = 378;

$mmLastProject = 0;
$mmLastScene = 0;  

$mmLoadedScenes = 0;
$mmLoadedShapes = 0;

$mmSelectedShape = 0;

$mmLoopDetecting = false;
$mmRotDeltaSumMin = 0;
$mmRotDeltaSumDescending = 0;
$mmRotDeltaSumLast = 0;

$mmKeyframesRotation = 1;
$mmKeyframesPosition = 0;

$mmSequenceBlend = 0;
$mmSequenceLoop = 0;
$mmSequenceGroundAnimate = 0;

$mmKeyframeID = 0;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////

function numericTest(%testString)
{
   if ((strlen(%testString)==0)||(!isAllNumeric(%testString)))
      return false;
   else 
      return true;  
}

//$lastScene = 0; //Store selected scene, for when we re-expose form, but we're working on a scene
//and project that is not the first one.
function exposeMegaMotionScenesForm()
{
   %project_id = 0;
   %scene_id = 0;
   %sceneShape_id = 0;
   %sequence_id = 0;
   %tab_page = 3;//SceneShapeTab=3, not BvhTab=0
   
   if (isDefined("MegaMotionScenes"))
   {
      %project_id = $mmProjectList.getSelected();
      %scene_id = $mmSceneList.getSelected();
      %sceneShape_id = $mmSceneShapeList.getSelected();
      %sequence_id = $mmSequenceList.getSelected();
      %tab_page = $mmTabBook.getSelectedPage();
      MegaMotionScenes.delete();
   }
   
   %dbRebuild = true;
   if (%dbRebuild)
      makeSqlGuiForm($MegaMotionScenesWindowID);
   else //Hmm, not working, not sure why.
      exec("../../MegaMotionScenes.gui");//FIX: find or make a directory for these, not game/.
      
   setupMegaMotionScenesForm();   
   
   if ((%project_id>0)&&(%project_id!=$mmProjectList.getSelected()))
      $mmProjectList.setSelected(%project_id);
   if ((%scene_id>0)&&(%scene_id!=$mmSceneList.getSelected()))
      $mmSceneList.setSelected(%scene_id);
   if ((%sceneShape_id>0)&&(%sceneShape_id!=$mmSceneShapeList.getSelected()))
      $mmSceneShapeList.setSelected(%sceneShape_id);
   if ((%sequence_id>0)&&(%sequence_id!=$mmSequenceList.getSelected()))
      $mmSequenceList.setSelected(%sequence_id);
      
   $mmTabBook.selectPage(%tab_page);
   
}

function openMegaMotionScenes()
{
   echo("calling openMegaMotionScenes");
}

function setupMegaMotionScenesForm()
{
   echo("calling setupMegaMotionScenesForm");
   
   if (!isDefined("MegaMotionScenes"))
      return;   
   
   $mmShapeId = 0;
   $mmPosId = 0;
   $mmRotId = 0;
   $mmScaleId = 0;
  
   $mmProjectList = MegaMotionScenes.findObjectByInternalName("projectList");
   $mmSceneList = MegaMotionScenes.findObjectByInternalName("sceneList");
   $mmSceneShapeList = MegaMotionScenes.findObjectByInternalName("sceneShapeList"); 
   $mmShapeList = MegaMotionScenes.findObjectByInternalName("shapeList"); 
    
   $mmTabBook = MegaMotionScenes.findObjectByInternalName("mmTabBook");    
   $mmTabBook.allowReorder = true;

   //maybe temporary   
   $mmSceneShapeTab = $mmTabBook.findObjectByInternalName("sceneShapeTab");
   $mmShapePartTab = $mmTabBook.findObjectByInternalName("shapePartTab");
   $mmSequenceTab = $mmTabBook.findObjectByInternalName("sequenceTab");
   $mmBvhTab = $mmTabBook.findObjectByInternalName("bvhTab");

   $mmShapePartTab.setTabIndex(0);
   $mmBvhTab.setTabIndex(1);
   $mmSequenceTab.setTabIndex(2);
   $mmSceneShapeTab.setTabIndex(3);   
   $mmTabBook.reArrange();   
   $mmTabBook.selectPage(3);
   
   setupMMSceneShapeTab();
   
   setupMMShapePartTab();
   
   setupMMSequenceTab();

   setupMMBvhTab();
   
}

function setupMMSceneShapeTab()
{
   %panel = $mmSceneShapeTab.findObjectByInternalName("sceneShapePanel");
   
   //Can we reduce the number of globals here, by defining them temporarily where needed?
   $mmSceneShapeBehaviorTree = %panel.findObjectByInternalName("sceneShapeBehaviorTree");
   $mmShapeGroupList = %panel.findObjectByInternalName("sceneShapeGroupList");
   $mmSceneShapePositionX = %panel.findObjectByInternalName("sceneShapePositionX");
   $mmSceneShapePositionY = %panel.findObjectByInternalName("sceneShapePositionY");
   $mmSceneShapePositionZ = %panel.findObjectByInternalName("sceneShapePositionZ");
   $mmSceneShapeOrientationX = %panel.findObjectByInternalName("sceneShapeOrientationX");//quat
   $mmSceneShapeOrientationY = %panel.findObjectByInternalName("sceneShapeOrientationY");
   $mmSceneShapeOrientationZ = %panel.findObjectByInternalName("sceneShapeOrientationZ");
   $mmSceneShapeOrientationAngle = %panel.findObjectByInternalName("sceneShapeOrientationAngle");
   $mmSceneShapeScaleX = %panel.findObjectByInternalName("sceneShapeScaleX");
   $mmSceneShapeScaleY = %panel.findObjectByInternalName("sceneShapeScaleY");
   $mmSceneShapeScaleZ = %panel.findObjectByInternalName("sceneShapeScaleZ");  
   $mmOpenSteerList = %panel.findObjectByInternalName("sceneShapeOpenSteerList");
}

function setupMMShapePartTab()
{
   %panel = $mmShapePartTab.findObjectByInternalName("shapePartPanel");
   
   $mmShapePartList = %panel.findObjectByInternalName("shapePartList");   
   $mmShapePartTypeList = %panel.findObjectByInternalName("shapePartTypeList");
   $mmShapePartBaseNodeList = %panel.findObjectByInternalName("shapePartBaseNodeList");   
   $mmShapePartChildNodeList = %panel.findObjectByInternalName("shapePartChildNodeList");   
   
   
   $mmShapePartTypeList.add("Box","0");
   $mmShapePartTypeList.add("Capsule","1");
   $mmShapePartTypeList.add("Sphere","2");
   //From here down, currently unsupported.
   //$mmShapeTypeList.add("Convex","3");
   //$mmShapeTypeList.add("Collision","4");
   //$mmShapeTypeList.add("Trimesh","5");
   $mmShapePartTypeList.setSelected(0);
   
   $mmJointList = %panel.findObjectByInternalName("jointList");
   
   $mmJointTypeList = %panel.findObjectByInternalName("jointTypeList");
   $mmJointTypeList.add("Spherical","0");
   $mmJointTypeList.add("Revolute","1");
   $mmJointTypeList.add("Prismatic","2");
   $mmJointTypeList.add("Fixed","3");
   $mmJointTypeList.add("Distance","4");
   $mmJointTypeList.add("D6","5");
   $mmJointTypeList.setSelected(5);
   
   
   
   %query = "SELECT id,name FROM project ORDER BY name;";  
   %resultSet = sqlite.query(%query, 0); 
   if (%resultSet)
   {
      if (sqlite.numRows(%resultSet)>0)
      {         
         %firstID = sqlite.getColumn(%resultSet, "id");
         while (!sqlite.endOfResult(%resultSet))
         {
            %id = sqlite.getColumn(%resultSet, "id");
            %name = sqlite.getColumn(%resultSet, "name");
            
            $mmProjectList.add(%name,%id);
            sqlite.nextRow(%resultSet);         
         }
         if ($mmLastProject>0)
            $mmProjectList.setSelected($mmLastProject);
         else if (%firstID>0) 
            $mmProjectList.setSelected(%firstID);
      }
      sqlite.clearResult(%resultSet);
   }
   
   %query = "SELECT id,name FROM physicsShape ORDER BY name;";
   %resultSet = sqlite.query(%query, 0);
   if (%resultSet)
   {
      if (sqlite.numRows(%resultSet)>0)
      {         
         //%firstID = sqlite.getColumn(%resultSet, "id");
         while (!sqlite.endOfResult(%resultSet))
         {
            %id = sqlite.getColumn(%resultSet, "id");
            %name = sqlite.getColumn(%resultSet, "name");
            
            $mmShapeList.add(%name,%id);
            sqlite.nextRow(%resultSet);         
         }
         //if (%firstID>0) 
            //$mmShapeList.setSelected(%firstID);
      }
      sqlite.clearResult(%resultSet);
   }   
   
   //behaviorList
   //Whoops, getting rid of the list for now, changing back to a text edit.
   
   //groupList
   %query = "SELECT id,name FROM shapeGroup ORDER BY name;";
   %resultSet = sqlite.query(%query, 0);
   if (%resultSet)
   {
      if (sqlite.numRows(%resultSet)>0)
      {         
         //%firstID = sqlite.getColumn(%resultSet, "id");
         while (!sqlite.endOfResult(%resultSet))
         {
            %id = sqlite.getColumn(%resultSet, "id");
            %name = sqlite.getColumn(%resultSet, "name");
            
            $mmShapeGroupList.add(%name,%id);
            sqlite.nextRow(%resultSet);         
         }
         //if (%firstID>0) 
            //$mmShapeList.setSelected(%firstID);
      }
      sqlite.clearResult(%resultSet);
   }
   
   %query = "SELECT id,name FROM openSteerProfile ORDER BY name;";
   %resultSet = sqlite.query(%query, 0);

   %id = "0";
   %name = "None";
   $mmOpenSteerList.add(%name @ "   " @ %id,%id);

   if (%resultSet)
   {
      if (sqlite.numRows(%resultSet)>0)
      {         
         //%firstID = sqlite.getColumn(%resultSet, "id");
         while (!sqlite.endOfResult(%resultSet))
         {
            %id = sqlite.getColumn(%resultSet, "id");
            %name = sqlite.getColumn(%resultSet, "name");
            
            $mmOpenSteerList.add(%name @ "   " @ %id,%id);
            sqlite.nextRow(%resultSet);         
         }
         //if (%firstID>0) 
            //$mmShapeList.setSelected(%firstID);
      }
      sqlite.clearResult(%resultSet);
   }   
   
   %query = "SELECT id,name FROM px3Joint ORDER BY name;";  
   %resultSet = sqlite.query(%query, 0); 
   if (%resultSet)
   {
      if (sqlite.numRows(%resultSet)>0)
      {         
         //%firstID = sqlite.getColumn(%resultSet, "id");
         while (!sqlite.endOfResult(%resultSet))
         {
            %id = sqlite.getColumn(%resultSet, "id");
            %name = sqlite.getColumn(%resultSet, "name");
            
            $mmJointList.add(%name,%id);
            sqlite.nextRow(%resultSet);         
         }
         //if (%firstID>0) 
            //$mmShapeList.setSelected(%firstID);
      }
      sqlite.clearResult(%resultSet);
   }   
}

function setupMMSequenceTab()
{
   %panel = $mmSequenceTab.findObjectByInternalName("sequencePanel");
   
   $mmSequenceList = %panel.findObjectByInternalName("sequenceList");
   $mmSequenceActionList = %panel.findObjectByInternalName("sequenceActionList");
   $mmSequenceFileText = %panel.findObjectByInternalName("sequenceFileText");
   
   //Can we reduce the number of globals here, by defining them temporarily where needed?
   $mmSequenceNodeList = %panel.findObjectByInternalName("sequenceNodeList");
   $mmSequenceAllNodeList = %panel.findObjectByInternalName("sequenceAllNodeList");
   $mmSequenceKeyframeSeriesList = %panel.findObjectByInternalName("sequenceKeyframeSeriesList");
   $mmSequenceKeyframeList = %panel.findObjectByInternalName("sequenceKeyframeList");
   
   $mmSequenceKeyframeValueX = %panel.findObjectByInternalName("sequenceKeyframeValueX");
   $mmSequenceKeyframeValueY = %panel.findObjectByInternalName("sequenceKeyframeValueY");
   $mmSequenceKeyframeValueZ = %panel.findObjectByInternalName("sequenceKeyframeValueZ");
   
   $mmSequenceKeyframeValueX.setText(0);
   $mmSequenceKeyframeValueY.setText(0);
   $mmSequenceKeyframeValueZ.setText(0);
   
   $mmLoopDetectorDelay = 10;
   $mmLoopDetectorMax   = 150;
   $mmLoopDetectorSmooth = 10;
   
   $mmGroundCaptureButton = %panel.findObjectByInternalName("groundCaptureButton");
   
   //%groundAnimateCheckbox = %panel.findObjectByInternalName("sequenceGroundAnimate");
   //%groundAnimateCheckbox.setVisible(false);//Turning this off till it gets hooked up.
   
   %query = "SELECT id,name FROM action ORDER BY name;";  
   %resultSet = sqlite.query(%query, 0); 
   if (%resultSet)
   {
      if (sqlite.numRows(%resultSet)>0)
      {         
         while (!sqlite.endOfResult(%resultSet))
         {
            %id = sqlite.getColumn(%resultSet, "id");
            %name = sqlite.getColumn(%resultSet, "name");
            
            $mmSequenceActionList.add(%name,%id);
            sqlite.nextRow(%resultSet);         
         }
      }
      sqlite.clearResult(%resultSet);
   }   
   
   if (!isObject(MegaMotionSequenceWindow))
   {
      makeSqlGuiForm($MegaMotionSequenceWindowID);
      setupMegaMotionSequenceWindow();
   }
   
}

function setupMMBvhTab()
{
   %panel = $mmBvhTab.findObjectByInternalName("bvhPanel");
   
   $mmBvhImportProfileList = %panel.findObjectByInternalName("bvhImportProfileList");
   $mmBvhExportProfileList = %panel.findObjectByInternalName("bvhExportProfileList");
   $mmBvhProfileList = %panel.findObjectByInternalName("bvhProfileList");
   
   $mmBvhModelNodeList = %panel.findObjectByInternalName("bvhModelNodesList");
   $mmBvhBvhNodeList = %panel.findObjectByInternalName("bvhBvhNodesList");
   $mmBvhLinkedNodesList = %panel.findObjectByInternalName("bvhLinkedNodesList");
   
   %query = "SELECT id,name FROM bvhProfile;";
   %resultSet = sqlite.query(%query,0);
   if (%resultSet)
   {
      %firstID = sqlite.getColumn(%resultSet, "id");
      while (!sqlite.endOfResult(%resultSet))
      {
         %id = sqlite.getColumn(%resultSet, "id");
         %name = sqlite.getColumn(%resultSet, "name");
            
         $mmBvhImportProfileList.add(%name,%id);
         $mmBvhExportProfileList.add(%name,%id);
         $mmBvhProfileList.add(%name,%id);
         
         sqlite.nextRow(%resultSet);   
      }
      sqlite.clearResult(%resultSet);
   }
   
}

function updateMegaMotionForm()
{
   //This is the big Commit Button at the top of the form. 
   //Making it commit whichever tab is on top, plus the 
   
   //NOTE: these need to be fixed anytime we add another tab.
   %bvhTab = 3;
   %sceneShapeTab = 2;      
   %partsTab = 1;
   %sequencesTab = 0;
   
   %selectedTab = $mmTabBook.getSelectedPage();
   
   echo("Tab book selected page: " @ %selectedTab);
   if (%selectedTab == %sceneShapeTab)
   {
      //scene shape data
      //Maybe, first check for each item to be worth saving, ie is valid at least.
      
      updateMMSceneShapeTab();

   }
   else if (%selectedTab == %partsTab)
   {
      //bodypart physics data
      %partId = $mmShapePartList.getSelected();
      if (%partId<=0)
         return;
      
      mmUpdateShapePartTab();      
      
   }
   else if (%selectedTab == %bvhTab)
   {
      //bvh profile data
      
      
      updateMMBvhTab();    
   }
   
   if ($mmLoadedScenes>0)
   {//FIX: need to track which scenes are currently loaded, and reload all of them and only them.)
      mmUnloadScene($mmSceneList.getSelected());
      mmLoadScene($mmSceneList.getSelected());
   }
}

function updateMMSceneShapeTab()
{
   %sceneShapeId = $mmSceneShapeList.getSelected();
   if (%sceneShapeId<=0)
      return;
      
   echo("UPDATING SCENE SHAPE TAB, behavior " @ $mmSceneShapeBehaviorTree.getText() );
   %pos_x = $mmSceneShapePositionX.getText();
   %pos_y = $mmSceneShapePositionY.getText();
   %pos_z = $mmSceneShapePositionZ.getText();
   %query = "UPDATE vector3 SET x=" @ %pos_x @ ",y=" @ %pos_y @ ",z=" @ %pos_z @ 
            " WHERE id=" @ $mmPosId @ ";";
   sqlite.query(%query, 0); 
   
   %rot_x = $mmSceneShapeOrientationX.getText();
   %rot_y = $mmSceneShapeOrientationY.getText();
   %rot_z = $mmSceneShapeOrientationZ.getText();
   %rot_a = $mmSceneShapeOrientationAngle.getText();
   %query = "UPDATE rotation SET x=" @ %rot_x @ ",y=" @ %rot_y @ ",z=" @ %rot_z @ 
             ",angle=" @ %rot_a @ " WHERE id=" @ $mmRotId @ ";";
   sqlite.query(%query, 0); 
   
   %scale_x = $mmSceneShapeScaleX.getText();
   %scale_y = $mmSceneShapeScaleY.getText();
   %scale_z = $mmSceneShapeScaleZ.getText();
   %query = "UPDATE vector3 SET x=" @ %scale_x @ ",y=" @ %scale_y @ ",z=" @ %scale_z @ 
            " WHERE id=" @ $mmScaleId @ ";";
   sqlite.query(%query, 0); 
   
   %group_id = $mmShapeGroupList.getSelected();
   if ((%group_id>0)&&(%group_id!=$mmShapeGroupId))
   {
      %query = "UPDATE sceneShape SET shapeGroup_id=" @ %group_id @ " WHERE id=" @ %sceneShapeId @ ";";
      sqlite.query(%query, 0); 
   }
   
   %behavior_tree = $mmSceneShapeBehaviorTree.getText();
   echo("trying to change behavior tree from " @ $mmSceneShapeBehaviorTreeOrig @ " to " @ %behavior_tree @ "!!!!!!!!!!!!!!!!");
   if ((strlen(%behavior_tree)>0) && (%behavior_tree!$="NULL") &&
            (%behavior_tree!$=$mmSceneShapeBehaviorTreeOrig))
   {
      %query = "UPDATE sceneShape SET behavior_tree='" @ %behavior_tree @ "' WHERE id=" @ %sceneShapeId @ ";";
      sqlite.query(%query, 0); 
   }  
   
   
   %openSteer_id = $mmOpenSteerList.getSelected();
   if ((%openSteer_id>0)&&(%openSteer_id!=$mmOpenSteerId))
   {
      %query = "UPDATE sceneShape SET openSteerProfile_id=" @ %openSteer_id @ " WHERE id=" @ %sceneShapeId @ ";";
      echo("changing openSteer profile! " @ %query);
      sqlite.query(%query, 0); 
      
   }
   
   if (%openSteer_id>0)
   {
      %tab = $mmTabBook.findObjectByInternalName("sceneShapeTab");
      %panel = %tab.findObjectByInternalName("sceneShapePanel");
   
      %mass = %panel.findObjectByInternalName("sceneShapeOpenSteerMass");
      %radius = %panel.findObjectByInternalName("sceneShapeOpenSteerRadius");
      %maxForce = %panel.findObjectByInternalName("sceneShapeOpenSteerMaxForce");
      %maxSpeed = %panel.findObjectByInternalName("sceneShapeOpenSteerMaxSpeed");
      %wanderChance = %panel.findObjectByInternalName("sceneShapeOpenSteerWanderChance");
      %wanderWeight = %panel.findObjectByInternalName("sceneShapeOpenSteerWanderWeight");
      %seekTarget = %panel.findObjectByInternalName("sceneShapeOpenSteerSeekTarget");
      %avoidTarget = %panel.findObjectByInternalName("sceneShapeOpenSteerAvoidTarget");
      %seekNeighbor = %panel.findObjectByInternalName("sceneShapeOpenSteerSeekNeighbor");
      %avoidNeighbor = %panel.findObjectByInternalName("sceneShapeOpenSteerAvoidNeighbor");
      %avoidEdge = %panel.findObjectByInternalName("sceneShapeOpenSteerAvoidEdge");
      %detectEdge = %panel.findObjectByInternalName("sceneShapeOpenSteerDetectEdge");      
      
      %query = "UPDATE openSteerProfile SET " @
               "mass=" @ %mass.getText() @ ",radius=" @ %radius.getText() @ 
               ",maxForce=" @ %maxForce.getText() @ ",maxSpeed=" @ %maxSpeed.getText() @ 
               ",wanderChance=" @ %wanderChance.getText() @ ",wanderWeight=" @ %wanderWeight.getText() @ 
               ",seekTargetWeight=" @ %seekTarget.getText() @ ",avoidTargetWeight=" @ %avoidTarget.getText() @ 
               ",seekNeighborWeight=" @ %seekNeighbor.getText() @ ",avoidNeighborWeight=" @ %avoidNeighbor.getText() @ 
               ",avoidNavMeshEdgeWeight=" @ %avoidEdge.getText() @ ",detectNavMeshEdgeRange=" @ %detectEdge.getText() @ 
               " WHERE id=" @ %openSteer_id @ ";";
      echo("Changing openSteer data: \n " @ %query);
      sqlite.query(%query,0);
   }
}

function updateMMShapePartTab()
{//First shape part properties, then joint properties...
//FIX: All of these fields need to check for blank spaces, will crash query.

   %tab = $mmTabBook.findObjectByInternalName("shapePartTab");
   %panel = %tab.findObjectByInternalName("shapePartPanel");
   
   %partId = $mmShapePartList.getSelected();
   %dimensionsX = %panel.findObjectByInternalName("shapePartDimensionsX").getText();
   %dimensionsY = %panel.findObjectByInternalName("shapePartDimensionsY").getText();
   %dimensionsZ = %panel.findObjectByInternalName("shapePartDimensionsZ").getText();   
   %orientationX = %panel.findObjectByInternalName("shapePartOrientationX").getText();
   %orientationY = %panel.findObjectByInternalName("shapePartOrientationY").getText();
   %orientationZ = %panel.findObjectByInternalName("shapePartOrientationZ").getText();   
   %offsetX = %panel.findObjectByInternalName("shapePartOffsetX").getText();
   %offsetY = %panel.findObjectByInternalName("shapePartOffsetY").getText();
   %offsetZ = %panel.findObjectByInternalName("shapePartOffsetZ").getText();
   
   %query = "UPDATE physicsShapePart SET ";
   %query = %query @ "dimensions_x=" @ %dimensionsX;
   %query = %query @ ",dimensions_y=" @ %dimensionsY;
   %query = %query @ ",dimensions_z=" @ %dimensionsZ;
   %query = %query @ ",orientation_x=" @ %orientationX;
   %query = %query @ ",orientation_y=" @ %orientationY;
   %query = %query @ ",orientation_z=" @ %orientationZ;   
   %query = %query @ ",offset_x=" @ %offsetX;
   %query = %query @ ",offset_y=" @ %offsetY;
   %query = %query @ ",offset_z=" @ %offsetZ;
   %query = %query @ " WHERE id=" @ %partId @ ";";
   sqlite.query(%query,0);
   
   %jointId = $mmJointList.getSelected();
   if (%jointId<=0)
      return;
      
   %twistLimit = %panel.findObjectByInternalName("jointTwistLimit").getText();
   %swingLimit = %panel.findObjectByInternalName("jointSwingLimit").getText();
   %swingLimit2 = %panel.findObjectByInternalName("jointSwingLimit2").getText();
   %xLimit = %panel.findObjectByInternalName("jointXLimit").getText();
   %yLimit = %panel.findObjectByInternalName("jointYLimit").getText();
   %zLimit = %panel.findObjectByInternalName("jointZLimit").getText();
   %axisX = %panel.findObjectByInternalName("jointAxisX").getText();
   %axisY = %panel.findObjectByInternalName("jointAxisY").getText();
   %axisZ = %panel.findObjectByInternalName("jointAxisZ").getText();
   %normalX = %panel.findObjectByInternalName("jointNormalX").getText();
   %normalY = %panel.findObjectByInternalName("jointNormalY").getText();
   %normalZ = %panel.findObjectByInternalName("jointNormalZ").getText();
   %twistSpring = %panel.findObjectByInternalName("jointTwistSpring").getText();
   %swingSpring = %panel.findObjectByInternalName("jointSwingSpring").getText();
   %springDamper = %panel.findObjectByInternalName("jointSpringDamper").getText();
   %motorSpring = %panel.findObjectByInternalName("jointMotorSpring").getText();
   %motorDamper = %panel.findObjectByInternalName("jointMotorDamper").getText();
   %maxForce = %panel.findObjectByInternalName("jointMaxForce").getText();
   %maxTorque = %panel.findObjectByInternalName("jointMaxTorque").getText();
   
   %query = "UPDATE px3Joint SET ";
   %query = %query @ "twistLimit=" @ %twistLimit;
   %query = %query @ ",swingLimit=" @ %swingLimit;
   %query = %query @ ",swingLimit2=" @ %swingLimit2;
   %query = %query @ ",xLimit=" @ %xLimit;
   %query = %query @ ",yLimit=" @ %yLimit;
   %query = %query @ ",zLimit=" @ %zLimit;
   %query = %query @ ",localAxis_x=" @ %axisX;
   %query = %query @ ",localAxis_y=" @ %axisY;
   %query = %query @ ",localAxis_z=" @ %axisZ;
   %query = %query @ ",localNormal_x=" @ %normalX;
   %query = %query @ ",localNormal_y=" @ %normalY;
   %query = %query @ ",localNormal_z=" @ %normalZ;
   %query = %query @ ",twistSpring=" @ %twistSpring;
   %query = %query @ ",swingSpring=" @ %swingSpring;
   %query = %query @ ",springDamper=" @ %springDamper;
   %query = %query @ ",motorSpring=" @ %motorSpring;
   %query = %query @ ",motorDamper=" @ %motorDamper;
   %query = %query @ ",maxForce=" @ %maxForce;
   %query = %query @ ",maxTorque=" @ %maxTorque;
   %query = %query @ " WHERE id=" @ %jointId @ ";";
   sqlite.query(%query,0);  
   
}

function updateMMBvhTab()
{
   
   %tab = $mmTabBook.findObjectByInternalName("bvhTab");
   %panel = %tab.findObjectByInternalName("bvhPanel");   
   
   %skeletonNodeId = $mmBvhLinkedNodesList.getSelected();
   
   %poseRotAX = %panel.findObjectByInternalName("bvhPoseRotAX").getText();
   %poseRotAY = %panel.findObjectByInternalName("bvhPoseRotAY").getText();
   %poseRotAZ = %panel.findObjectByInternalName("bvhPoseRotAZ").getText();  
   %poseRotBX = %panel.findObjectByInternalName("bvhPoseRotBX").getText();
   %poseRotBY = %panel.findObjectByInternalName("bvhPoseRotBY").getText();
   %poseRotBZ = %panel.findObjectByInternalName("bvhPoseRotBZ").getText();   
   %fixRotAX = %panel.findObjectByInternalName("bvhFixRotAX").getText();
   %fixRotAY = %panel.findObjectByInternalName("bvhFixRotAY").getText();
   %fixRotAZ = %panel.findObjectByInternalName("bvhFixRotAZ").getText();   
   %fixRotBX = %panel.findObjectByInternalName("bvhFixRotBX").getText();
   %fixRotBY = %panel.findObjectByInternalName("bvhFixRotBY").getText();
   %fixRotBZ = %panel.findObjectByInternalName("bvhFixRotBZ").getText(); 
   
   %query = "UPDATE bvhProfileSkeletonNode SET ";
   %query = %query @ "poseRotA_x=" @ %poseRotAX;
   %query = %query @ ",poseRotA_y=" @ %poseRotAY;
   %query = %query @ ",poseRotA_z=" @ %poseRotAZ;
   %query = %query @ ",poseRotB_x=" @ %poseRotBX;
   %query = %query @ ",poseRotB_y=" @ %poseRotBY;
   %query = %query @ ",poseRotB_z=" @ %poseRotBZ;
   %query = %query @ ",fixRotA_x=" @ %fixRotAX;
   %query = %query @ ",fixRotA_y=" @ %fixRotAY;
   %query = %query @ ",fixRotA_z=" @ %fixRotAZ;
   %query = %query @ ",fixRotB_x=" @ %fixRotBX;
   %query = %query @ ",fixRotB_y=" @ %fixRotBY;
   %query = %query @ ",fixRotB_z=" @ %fixRotBZ;
   %query = %query @ " WHERE id=" @ %skeletonNodeId @ ";";
   //echo(%query);
   sqlite.query(%query,0);  //HMMM this seems to cause a crash, but not until we're off in the 
   // keyframes section on reload. (???)
   
   
}


////////////////////////////////////////////////////////////////////////////////////
//REFACTOR: Still working out the MegaMotion vs openSimEarth division of labor.

//Direct copy of EditorSaveMission from menuHandlers.ed.cs. This version exists
//because mission save is actually just SimObject::save, and that is way too deep 
//into T3D to be making application level changes. Instead, we just call this one,
//but we are still going to have problems with all the plugins until we keep them 
//from calling MissionGroup.save() on their own.

function MegaMotionSaveMission()
{
   echo("Calling MegaMotionSaveMission!!!!!!!!!!!!!!!!!!!!!!!!!!!");
   //if(isFunction("getObjectLimit") && MissionGroup.getFullCount() >= getObjectLimit())
   //{ //(Object limit in trial version, ossible way to nag licensing compliance?)
   //   MessageBoxOKBuy( "Object Limit Reached", "You have exceeded the object limit of " @ getObjectLimit() @ " for this demo. You can remove objects if you would like to add more.", "", "Canvas.showPurchaseScreen(\"objectlimit\");" );
   //   return;
   //}
   
   // first check for dirty and read-only files:
   if((EWorldEditor.isDirty || ETerrainEditor.isMissionDirty) && !isWriteableFileName($Server::MissionFile))
   {
      MessageBox("Error", "Mission file \""@ $Server::MissionFile @ "\" is read-only.  Continue?", "Ok", "Stop");
      return false;
   }
   if(ETerrainEditor.isDirty)
   {
      // Find all of the terrain files
      initContainerTypeSearch($TypeMasks::TerrainObjectType);

      while ((%terrainObject = containerSearchNext()) != 0)
      {
         if (!isWriteableFileName(%terrainObject.terrainFile))
         {
            if (MessageBox("Error", "Terrain file \""@ %terrainObject.terrainFile @ "\" is read-only.  Continue?", "Ok", "Stop") == $MROk)
               continue;
            else
               return false;
         }
      }
   }
  
   // now write the terrain and mission files out:
   
   
   ///////////////////////////   
   //For terrainPager/openSimEarth, we need to save many things to the database instead of to 
   //the mission. Do not do this if we don't have a TerrainPager.
   if (isObject(theTP))//FIX!!! Search for objects of type TerrainPager, regardless of name.
   {
      %tempStaticGroup = new SimSet();
      %tempRoadGroup = new SimSet();
      %tempForestGroup = new SimSet();
      %tempSceneShapes = new SimSet();
      
      if ($pref::OpenSimEarth::saveSceneShapes)
         osePullSceneShapesAndSave(%tempSceneShapes);
      else
         osePullSceneShapes(%tempSceneShapes);
      
      if ($pref::OpenSimEarth::saveStatics)
         osePullStaticsAndSave(%tempStaticGroup);
      else
         osePullStatics(%tempStaticGroup);
      
      if ($pref::OpenSimEarth::saveRoads)
         osePullRoadsAndSave(%tempRoadGroup);
      else
         osePullRoads(%tempRoadGroup);
      
      //if ($pref::OpenSimEarth::saveForests==false)
      //   osePullForest($tempForestGroup);
   
   }
   
   if(EWorldEditor.isDirty || ETerrainEditor.isMissionDirty)
      MissionGroup.save($Server::MissionFile);
      
   if (isObject(theTP))//FIX!!! Search for objects of type TerrainPager, regardless of name.
   {   
      osePushSceneShapes(%tempSceneShapes);
      osePushStatics(%tempStaticGroup);
      osePushRoads(%tempRoadGroup);
      //osePushForest($tempForestGroup);
   }
   
   ///////////////////////////   
   
   
   if(ETerrainEditor.isDirty)
   {
      // Find all of the terrain files
      initContainerTypeSearch($TypeMasks::TerrainObjectType);

      while ((%terrainObject = containerSearchNext()) != 0)
         %terrainObject.save(%terrainObject.terrainFile);
   }

   ETerrainPersistMan.saveDirty();
      
   // Give EditorPlugins a chance to save.
   for ( %i = 0; %i < EditorPluginSet.getCount(); %i++ )
   {
      %obj = EditorPluginSet.getObject(%i);
      if ( %obj.isDirty() )
         %obj.onSaveMission( $Server::MissionFile );      
   } 
   
   EditorClearDirty();
   
   EditorGui.saveAs = false;
   
   return true;
}

function MegaMotionSaveSceneShapes()
{
   //NOW: find all physicsShapes, and save each of them 
   for (%i = 0; %i < SceneShapes.getCount();%i++)
   {
      %obj = SceneShapes.getObject(%i);  
      if ((%obj.sceneShapeID>0)&&(%obj.sceneID>0)&&(%obj.isDirty))
      {
         %query = "SELECT  pos_id,rot_id,scale_id FROM sceneShape " @ 
                  "WHERE id=" @ %obj.sceneShapeID @ ";";
         %resultSet = sqlite.query(%query,0);
         if (sqlite.numRows(%resultSet)==1)
         {
            %pos_id = sqlite.getColumn(%resultSet, "pos_id");
            %rot_id = sqlite.getColumn(%resultSet, "rot_id");
            %scale_id = sqlite.getColumn(%resultSet, "scale_id");
            
            %trans = %obj.getTransform();
            %p_x = getWord(%trans,0);
            %p_y = getWord(%trans,1);
            %p_z = getWord(%trans,2);
            %r_x = getWord(%trans,3);
            %r_y = getWord(%trans,4);
            %r_z = getWord(%trans,5);
            %r_angle = mRadToDeg(getWord(%trans,6));
            %query = "UPDATE vector3 SET x=" @ %p_x @ ",y=" @ %p_y @ ",z=" @ %p_z @
                     " WHERE id=" @ %pos_id @ ";";
            sqlite.query(%query,0);
            %query = "UPDATE rotation SET x=" @ %r_x @ ",y=" @ %r_y @ ",z=" @ %r_z @
                     ",angle=" @ %r_angle @ " WHERE id=" @ %rot_id @ ";";
            sqlite.query(%query,0);
            
            %scale = %obj.getScale();
            //if (%scale $= "1 1 1") //Do what now? we need to check for this and assign id = 1, and maintain that.
            %s_x = getWord(%scale,0);//Or, just ignore it and accept the bloat.
            %s_y = getWord(%scale,1);
            %s_z = getWord(%scale,2);            
            %query = "UPDATE vector3 SET x=" @ %s_x @ ",y=" @ %s_y @ ",z=" @ %s_z @
                     " WHERE id=" @ %scale_id @ ";";
            sqlite.query(%query,0);
            
            echo("updated a dirty scene shape!!");
         }        
      }
   }
}

////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////

function mmSelectProject()
{   
   echo("calling selectMegaMotionProject " @ $mmProjectList.getSelected());
   
   if ($mmProjectList.getSelected()<=0)
      return;
    
   $mmLastProject = $mmProjectList.getSelected();
   $mmSceneList.clear();  
   
   %firstID = 0;
   %query = "SELECT id,name FROM scene WHERE project_id=" @ $mmProjectList.getSelected() @ " ORDER BY name;";  
   %resultSet = sqlite.query(%query, 0); 
   if (%resultSet)
   {
      if (sqlite.numRows(%resultSet)>0)
      {
         %firstID = sqlite.getColumn(%resultSet, "id");
         while (!sqlite.endOfResult(%resultSet))
         {
            %id = sqlite.getColumn(%resultSet, "id");
            %name = sqlite.getColumn(%resultSet, "name");
            
            $mmSceneList.add(%name,%id);
            sqlite.nextRow(%resultSet);         
         }         
         if ($mmLastScene>0)
            $mmSceneList.setSelected($mmLastScene);
         else if (%firstID>0)
            $mmSceneList.setSelected(%firstID);
      }
      sqlite.clearResult(%resultSet);
   }      
}

function mmAddProject()
{
   makeSqlGuiForm($mmAddProjectWindowID);
}

function mmReallyAddProject()
{
   if (mmAddProjectWindow.isVisible())
   {
      %name = mmAddProjectWindow.findObjectByInternalName("nameEdit").getText(); 
      if ((strlen(%name)==0)||(substr(%name," ")>0))
      {
         MessageBoxOK("Name Invalid","Project name must be a unique character string with no spaces or special characters.","");
         return;  
      }
      %query = "SELECT id FROM project WHERE name='" @ %name @ "';";
      %resultSet = sqlite.query(%query,0);
      if (sqlite.numRows(%resultSet)>0)
      {
         MessageBoxOK("Name Invalid","Project name must be unique.","");
         return;
      }
      %query = "INSERT INTO project (name) VALUES ('" @ %name @ "');";
      sqlite.query(%query,0);
      
      mmAddProjectWindow.delete();
      
      exposeMegaMotionScenesForm();
   }
}

function mmDeleteProject()
{
   %project_id = $mmProjectList.getSelected();
   if (%project_id<=0)
      return;
      
   MessageBoxOKCancel( "Warning", 
      "This will permanently delete this project and all of its scenes! Are you completely sure?", 
      "mmReallyDeleteProject();",
      "" ); 
      
}
   
function mmReallyDeleteProject()
{
   %project_id = $mmProjectList.getSelected();
      
   %query = "SELECT id FROM scene WHERE project_id=" @ %project_id @ ";";
   %resultSet = sqlite.query(%query,0);
   if (sqlite.numRows(%resultSet)>0)
   {
      while (!sqlite.endOfResult(%resultSet))
      {
         mmReallyDeleteScene(sqlite.getColumn(%resultSet,"id") );
         sqlite.nextRow(%resultSet);  
      }
      sqlite.clearResult(%resultSet);      
   }
   
   %query = "DELETE FROM scene WHERE project_id=" @ %project_id @ ";";
   sqlite.query(%query,0);
   
   %query = "DELETE FROM project WHERE id=" @ %project_id @ ";";
   sqlite.query(%query,0);
   
   exposeMegaMotionScenesForm();
}


function mmLoadProject()
{
   //Maybe we will want to load shared static object props used by multiple scenes? 
    
}

function mmUnloadProject()
{
   //Remove project environment objects?
   
}

function mmSelectScene()
{
   echo("calling selectMegaMotionScene");
   
   if ($mmSceneList.getSelected()<=0)
      return;
      
   $mmLastScene = $mmSceneList.getSelected();
   $mmSceneShapeList.clear(); 
   
   echo("calling selectMegaMotionScene on scene " @ $mmSceneList.getSelected());
   
   %firstID = 0;
   %query = "SELECT ss.id,ps.id AS ps_id, ps.name FROM sceneShape ss " @
	         "JOIN physicsShape ps ON ps.id=ss.shape_id " @
            "WHERE scene_id=" @ $mmSceneList.getSelected() @ ";";  
   %resultSet = sqlite.query(%query, 0); 
   if (%resultSet)
   {
      //echo("adding " @ sqlite.numRows(%resultSet) @ " scene shapes!");
      if (sqlite.numRows(%resultSet)>0)
      {
         %firstID = sqlite.getColumn(%resultSet, "id");
         while (!sqlite.endOfResult(%resultSet))
         {
            %id = sqlite.getColumn(%resultSet, "id");
            %name = sqlite.getColumn(%resultSet, "name") @ " - " @ %id;
            $mmSceneShapeList.add(%name,%id);
            sqlite.nextRow(%resultSet);         
         }
         //if (%firstID>0) //Don't do this after all, because it fills up the sceneShape tab before the
         //   $mmSceneShapeList.setSelected(%firstID);// user has actually selected a shape.
      }
      sqlite.clearResult(%resultSet);
   }   
}

function mmAddScene()
{
   makeSqlGuiForm($mmAddSceneWindowID);   
}

function mmReallyAddScene()  //TO DO: Description, position.
{  
   if (mmAddSceneWindow.isVisible())
   {
      %name = mmAddSceneWindow.findObjectByInternalName("nameEdit").getText(); 
      if ((strlen(%name)==0)||(substr(%name," ")>0))
      {
         MessageBoxOK("Name Invalid","Scene name must be a unique character string with no spaces or special characters.","");
         return;  
      }
      %proj_id = $mmProjectList.getSelected();
      %query = "SELECT id FROM scene WHERE name='" @ %name @ "' AND project_id=" @ %proj_id @ ";";
      %resultSet = sqlite.query(%query,0);
      if (sqlite.numRows(%resultSet)>0)
      {
         MessageBoxOK("Name Invalid","Scene name must be unique for this project.","");
         return;
      }
      sqlite.clearResult(%resultSet);
      
      //HERE: need scene position XYZ fields.
      %query = "INSERT INTO vector3 (x,y,z) VALUES (0,0,0);";
      sqlite.query(%query,0);      
      
      %query = "INSERT INTO scene (name,project_id,pos_id) VALUES ('" @ %name @ "'," @ %proj_id @ 
                  ",last_insert_rowid());";
      sqlite.query(%query,0);
            
      mmAddSceneWindow.delete();
      
      exposeMegaMotionScenesForm();
   }
}

function mmDeleteScene()
{
   %scene_id = $mmSceneList.getSelected();
   if (%scene_id<=0)
      return;
      
   MessageBoxOKCancel( "Warning", 
      "This will permanently delete this scene and all of its sceneShapes! Are you completely sure?", 
      "mmReallyDeleteScene(" @ %scene_id @ ");",
      "" );    
}

function mmReallyDeleteScene(%id)
{
   mmUnloadScene(%id);
   
   %query = "SELECT id FROM sceneShape WHERE scene_id=" @ %id @ ";";
   %resultSet = sqlite.query(%query,0);
   if (sqlite.numRows(%resultSet)>0)
   {
      while (!sqlite.endOfResult(%resultSet))
      {
         %shape_id = sqlite.getColumn(%resultSet,"id");
         mmReallyDeleteSceneShape(%shape_id);
         sqlite.nextRow(%resultSet); 
      }
      sqlite.clearResult(%resultSet);
   }
   
   %query = "DELETE FROM scene WHERE id=" @ %id @ ";";
   sqlite.query(%query,0);
   
   exposeMegaMotionScenesForm();
}

function mmLoadScene(%id)
{      
   if (%id<=0)
      return;
      
   //FIRST: check to see if we've already loaded this scene! Which we are going
   //to do by checking if there are any existing sceneShapes from this scene.
   for (%i = 0; %i < SceneShapes.getCount();%i++)
   {
      %obj = SceneShapes.getObject(%i); 
      if (%obj.sceneID==%id)
      {
         MessageBoxOK("Scene already loaded.","Shapes from this scene are already present. Unload scene before adding again.","");
         return;  
      }
   }
   
   %dyn = false;
   %grav = true;
   %ambient = true;

	%query = "SELECT ss.id as ss_id,shape_id,shapeGroup_id,behavior_tree,openSteerProfile_id," @ 
	         "actionProfile_id,p.x as pos_x,p.y as pos_y,p.z as pos_z," @ 
	         "r.x as rot_x,r.y as rot_y,r.z as rot_z,r.angle as rot_angle," @ 
	         "sc.x as scale_x,sc.y as scale_y,sc.z as scale_z," @ 
	         "sp.x as scene_pos_x,sp.y as scene_pos_y,sp.z as scene_pos_z," @ 
	         "sh.datablock as datablock, sf.skeleton_id AS skeleton_id " @ 
	         "FROM sceneShape ss " @ 
	         "JOIN scene s ON s.id=scene_id " @
	         "LEFT JOIN vector3 p ON ss.pos_id=p.id " @ 
	         "LEFT JOIN rotation r ON ss.rot_id=r.id " @ 
	         "LEFT JOIN vector3 sc ON ss.scale_id=sc.id " @ 
	         "LEFT JOIN vector3 sp ON s.pos_id=sp.id " @ 
	         "JOIN physicsShape sh ON ss.shape_id=sh.id " @ 
	         "JOIN shapeFile sf ON sh.file_id=sf.id " @ 
	         "WHERE scene_id=" @ %id @ ";";  
	%resultSet = sqlite.query(%query, 0);
	
	echo("calling loadScene, result " @ %resultSet);
   echo( "Query: " @ %query );	
	
   if (%resultSet)
   {
      while (!sqlite.endOfResult(%resultSet))
      {
         %sceneShape_id = sqlite.getColumn(%resultSet, "ss_id");   
         %shape_id = sqlite.getColumn(%resultSet, "shape_id");
         %shapeGroup_id = sqlite.getColumn(%resultSet, "shapeGroup_id");//not used yet
         %behaviorTree = sqlite.getColumn(%resultSet, "behavior_tree");
         %openSteer_id = sqlite.getColumn(%resultSet, "openSteerProfile_id");
         %actionProfile_id = sqlite.getColumn(%resultSet, "actionProfile_id");
         
         %pos_x = sqlite.getColumn(%resultSet, "pos_x");
         %pos_y = sqlite.getColumn(%resultSet, "pos_y");
         %pos_z = sqlite.getColumn(%resultSet, "pos_z");
         
         %rot_x = sqlite.getColumn(%resultSet, "rot_x");
         %rot_y = sqlite.getColumn(%resultSet, "rot_y");
         %rot_z = sqlite.getColumn(%resultSet, "rot_z");
         %rot_angle = sqlite.getColumn(%resultSet, "rot_angle");
         
         %scale_x = sqlite.getColumn(%resultSet, "scale_x");
         %scale_y = sqlite.getColumn(%resultSet, "scale_y");
         %scale_z = sqlite.getColumn(%resultSet, "scale_z");
         
         %scene_pos_x = sqlite.getColumn(%resultSet, "scene_pos_x");
         %scene_pos_y = sqlite.getColumn(%resultSet, "scene_pos_y");
         %scene_pos_z = sqlite.getColumn(%resultSet, "scene_pos_z");
         
         %datablock = sqlite.getColumn(%resultSet, "datablock");
         %skeleton_id = sqlite.getColumn(%resultSet, "skeleton_id");
         
         echo("Found a sceneShape: " @ %sceneShape_id @ " " @ %pos_x @ " " @ %pos_y @ " " @ %pos_z @
                " scenePos " @ %scene_pos_x @ " " @ %scene_pos_y @ " " @ %scene_pos_z );
                
         %position = (%pos_x + %scene_pos_x) @ " " @ (%pos_y + %scene_pos_y) @ " " @ (%pos_z + %scene_pos_z);
         %rotation = %rot_x @ " " @ %rot_y @ " " @ %rot_z @ " " @ %rot_angle;
         %scale = %scale_x @ " " @ %scale_y @ " " @ %scale_z;
         
         echo("loading sceneShape id " @ %shape_id @ " position " @ %position @ " rotation " @ 
               %rotation @ " scale " @ %scale);
         
         //TEMP
         %name = "";          
         if (%shape_id==4)
            %name = "ka50";   
         else if (%shape_id==3)
            %name = "bo105";
         else if (%shape_id==2)
            %name = "dragonfly";
            
         %temp =  new PhysicsShape(%name) {
            playAmbient = %ambient;
            dataBlock = %datablock;
            position = %position;
            rotation = %rotation;
            scale = %scale;
            canSave = "1";
            canSaveDynamicFields = "1";
            areaImpulse = "0";
            damageRadius = "0";
            invulnerable = "0";
            minDamageAmount = "0";
            radiusDamage = "0";
            hasGravity = %grav;
            isDynamic = %dyn;
            shapeID = %shape_id;
            sceneShapeID = %sceneShape_id;
            sceneID = %id;
            openSteerID = %openSteer_id;
            actionProfileID = %actionProfile_id;
            skeletonID = %skeleton_id;
            targetType = "Health";//"AmmoClip" "Player"
            isDirty = false;
         };
         
         MissionGroup.add(%temp);   
         SceneShapes.add(%temp);   
         
         echo("Adding a scene shape: " @ %sceneShape_id @ ", position " @ %position );
         
         if ((strlen(%behaviorTree)>0)&&(%behaviorTree!$="NULL"))
         {
            %temp.schedule(30,"setBehavior",%behaviorTree);
            //echo(%temp.getId() @ " assigning behavior tree: " @ %behaviorTree );
         }
         sqlite.nextRow(%resultSet);
      }
   }   
   sqlite.clearResult(%resultSet);
   
   $mmLoadedScenes++;
   
   schedule(40, 0, "loadMMKeyframeSets");
} 

function mmUnloadScene(%id)
{
   if (%id<=0)
      return;
      
   //HERE: look up all the sceneShapes from the scene in question, and drop them all from the current mission.
   %shapesCount = SceneShapes.getCount();
   for (%i=0;%i<%shapesCount;%i++)
   {
      %shape = SceneShapes.getObject(%i);  
      //echo("shapesCount " @ %shapesCount @ ", sceneShape id " @ %shape.sceneShapeID @ 
      //         " scene " @ %shape.sceneID ); 
      if (%shape.sceneID==%id)
      {       
         MissionGroup.remove(%shape);
         SceneShapes.remove(%shape);//Wuh oh... removing from SceneShapes shortens the array...
         %shape.delete();//Maybe??
         
         %shapesCount = SceneShapes.getCount();
         if (%shapesCount>0)
            %i=-1;//So start over every time we remove one, until we loop through and remove none.
         else 
            %i=1;//Or else we run out of shapes, and just need to exit the loop.   
            
         $mmLoadedScenes--;
      }
   }   
}
function mmLoadKeyframeSets()
{   
   %scene_id = $mmSceneList.getSelected();
   if ((%scene_id<=0)||(SceneShapes.getCount()==0))
      return;
  
  //First, make a distinct list of existing shapes, discarding duplicates.
   %numShapes = 0; //(Consider making this list global so we have it later.)
   for (%i=0;%i<SceneShapes.getCount();%i++)
   {
      %shape = SceneShapes.getObject(%i);
      %shape_id = %shape.shapeID;  
      if (%numShapes==0)
      {
         %shapes[%numShapes] = %shape;
         %shape_ids[%numShapes] = %shape_id;
         %numShapes++;
      }
      else
      {
         %found = 0;
         for (%j=0;%j<%numShapes;%j++)
            if (%shape_ids[%j]==%shape_id)
               %found=1;         
         if (%found==0)
         {
            %shapes[%numShapes] = %shape;
            %shape_ids[%numShapes] = %shape_id;
            %numShapes++;
         }
      }
   }
   if (%numShapes==0)
      return;

   for (%i=0;%i<%numShapes;%i++)
   {
      %shape = %shapes[%i];
      %shape_id = %shape_ids[%i];
      
      %query = "SELECT * FROM keyframeSet WHERE shape_id=" @ %shape_id @ ";";
      %resultSet = sqlite.query(%query,0);        
      while (!sqlite.endOfResult(%resultSet))
      { 
         %set_id = sqlite.getColumn(%resultSet,"id");
         %sequence = sqlite.getColumn(%resultSet,"sequence_name");
         %name = sqlite.getColumn(%resultSet,"name");
         
         %shape.addKeyframeSet(%sequence);
         
         %query2 = "SELECT * FROM keyframeSeries WHERE set_id=" @ %set_id @ ";";
         %resultSet2 = sqlite.query(%query2,0);
         while (!sqlite.endOfResult(%resultSet2))
         {
            %series_id = sqlite.getColumn(%resultSet2,"id");
            %type = sqlite.getColumn(%resultSet2,"type");
            %node = sqlite.getColumn(%resultSet2,"node");
            
            %shape.addKeyframeSeries(%type,%node);
            
            %query3 = "SELECT * FROM keyframe WHERE series_id=" @ %series_id @ " ORDER BY frame;";
            %resultSet3 = sqlite.query(%query3,0);
            while (!sqlite.endOfResult(%resultSet3))
            {
               %frame = sqlite.getColumn(%resultSet3,"frame");
               %x = sqlite.getColumn(%resultSet3,"x");
               %y = sqlite.getColumn(%resultSet3,"y");
               %z = sqlite.getColumn(%resultSet3,"z");
               
               %shape.addKeyframe(%frame,%x,%y,%z);
               
               sqlite.nextRow(%resultSet3); 
            }            
            sqlite.nextRow(%resultSet2); 
         }
         
         %shape.applyKeyframeSet();
         
         sqlite.nextRow(%resultSet);          
      }
   }   
}

function mmSelectSceneShape()
{
   echo("selecting mm scene shape!!!!");
   if ($mmSceneShapeList.getSelected()<=0)
      return;
        
   %scene_shape_id = $mmSceneShapeList.getSelected();
   
   $mmSequenceList.clear();
   for (%i = 0; %i < SceneShapes.getCount();%i++)
   {
      %obj = SceneShapes.getObject(%i);  
      if (%obj.sceneShapeID==%scene_shape_id)
      {
         $mmSelectedShape = %obj;       
         %numSeqs = $mmSelectedShape.getNumSeqs();
         for (%j=0;%j<%numSeqs;%j++)
         {
            %name = $mmSelectedShape.getSeqName(%j);
            $mmSequenceList.add(%name,%j);         
         }
      }
   }
   
   //%query = "SELECT * FROM sceneShape WHERE id=" @ %sceneShapeId @ ";";  
	%query = "SELECT shape_id,ss.name,shapeGroup_id,behavior_tree,openSteerProfile_id," @ 
	         "ss.pos_id AS pos_id,p.x AS pos_x,p.y AS pos_y,p.z AS pos_z," @ 
	         "ss.rot_id AS rot_id,r.x AS rot_x,r.y AS rot_y,r.z AS rot_z,r.angle AS rot_angle," @ 
	         "ss.scale_id AS scale_id,sc.x AS scale_x,sc.y AS scale_y,sc.z AS scale_z " @ 
	         "FROM sceneShape ss " @ 
	         "LEFT JOIN vector3 p ON ss.pos_id=p.id " @ 
	         "LEFT JOIN rotation r ON ss.rot_id=r.id " @ 
	         "LEFT JOIN vector3 sc ON ss.scale_id=sc.id " @ 
	         "WHERE ss.id=" @ %scene_shape_id @ ";";

   %resultSet = sqlite.query(%query, 0); 
   if (sqlite.numRows(%resultSet)==1)
   {
      %name = sqlite.getColumn(%resultSet, "name");
      %behavior_tree = sqlite.getColumn(%resultSet, "behavior_tree");
      $mmSceneShapeBehaviorTreeOrig = %behavior_tree;
      %shape_id = sqlite.getColumn(%resultSet, "shape_id");
      $mmShapeId = %shape_id;
      %group_id = sqlite.getColumn(%resultSet, "shapeGroup_id");
      $mmShapeGroupId = %group_id;
      %openSteer_id = sqlite.getColumn(%resultSet, "openSteerProfile_id");
      $mmOpenSteerId = %openSteer_id;
      %pos_x = sqlite.getColumn(%resultSet, "pos_x");
      %pos_y = sqlite.getColumn(%resultSet, "pos_y");
      %pos_z = sqlite.getColumn(%resultSet, "pos_z");
      $mmPosId = sqlite.getColumn(%resultSet, "pos_id");
      %rot_x = sqlite.getColumn(%resultSet, "rot_x");
      %rot_y = sqlite.getColumn(%resultSet, "rot_y");
      %rot_z = sqlite.getColumn(%resultSet, "rot_z");
      %rot_a = sqlite.getColumn(%resultSet, "rot_angle");
      $mmRotId = sqlite.getColumn(%resultSet, "rot_id");
      %scale_x = sqlite.getColumn(%resultSet, "scale_x");
      %scale_y = sqlite.getColumn(%resultSet, "scale_y");
      %scale_z = sqlite.getColumn(%resultSet, "scale_z");
      $mmScaleId = sqlite.getColumn(%resultSet, "scale_id");
      
      $mmShapeList.setSelected(%shape_id);
      
      $mmSceneShapeBehaviorTree.setText(%behavior_tree); 
      $mmShapeGroupList.setSelected(%group_id);
      $mmOpenSteerList.setSelected(%openSteer_id);
      
      $mmSceneShapePositionX.setText(%pos_x);
      $mmSceneShapePositionY.setText(%pos_y);
      $mmSceneShapePositionZ.setText(%pos_z);
      
      $mmSceneShapeOrientationX.setText(%rot_x);
      $mmSceneShapeOrientationY.setText(%rot_y);
      $mmSceneShapeOrientationZ.setText(%rot_z);
      $mmSceneShapeOrientationAngle.setText(%rot_a);
      
      $mmSceneShapeScaleX.setText(%scale_x);
      $mmSceneShapeScaleY.setText(%scale_y);
      $mmSceneShapeScaleZ.setText(%scale_z);
      
      sqlite.clearResult(%resultSet);
   }
}

function mmAddSceneShape()
{
   makeSqlGuiForm($mmAddSceneShapeWindowID);   
   setupMMAddSceneShapeForm();
}

function mmDeleteSceneShape()
{   
   if ($mmSceneShapeList.getSelected()<=0)
      return;

   //Don't bother with warning message for every scene shape.
   mmReallyDeleteSceneShape($mmSceneShapeList.getSelected());
   
   exposeMegaMotionScenesForm();   
   
   if ($mmLoadedScenes>0)
   {
      mmUnloadScene($mmSceneList.getSelected());
      mmLoadScene($mmSceneList.getSelected());
   }
}

function mmReallyDeleteSceneShape(%id)
{//Now, this can get called from the delete button, OR from inside a loop in another function.
   
   %query = "SELECT pos_id,rot_id,scale_id FROM sceneShape WHERE id=" @ %id @ ";";
   %resultSet = sqlite.query(%query,0);
   if (%resultSet==0)
      return;

   %pos_id = sqlite.getColumn(%resultSet,"pos_id");
   %rot_id = sqlite.getColumn(%resultSet,"rot_id");
   %scale_id = sqlite.getColumn(%resultSet,"scale_id");
   sqlite.clearResult(%resultSet);
   
   %query = "DELETE FROM vector3 WHERE id=" @ %pos_id @ " OR id=" @ %scale_id @ ";";
   sqlite.query(%query,0);
   %query = "DELETE FROM rotation WHERE id=" @ %rot_id @ ";";
   sqlite.query(%query,0);
   %query = "DELETE FROM sceneShape WHERE id=" @ %id @ ";";
   sqlite.query(%query,0);//Here, not sure it's a speed advantage, but piling queries 
                           //together whenever possible just in case. (didn't work?)
}


function mmSelectShapeGroup()
{
   //select all instantiated members of this group
}

function mmAddShape()
{
   //HERE: file browser window
}

function mmReallyAddShape()
{
   
}

function mmDeleteShape()
{   //First, delete all the shapeParts, then the shape.
   if ($mmShapeList.getSelected()<=0)
      return;
      
   MessageBoxOKCancel( "Warning", 
      "This will permanently delete this shape and all shapeParts referencing it. Are you completely sure?", 
      "mmReallyDeleteShape();",
      "" );     
}

function mmReallyDeleteShape()
{
   if ($mmShapeList.getSelected()<=0)
      return;
      
   %shape_id = $mmShapeList.getSelected();
   
   %query = "DELETE FROM physicsShapePart WHERE physicsShape_id=" @ %shape_id @ ";";
   sqlite.query(%query);
   
   %query = "DELETE FROM shapeMount WHERE parent_shape_id=" @ %shape_id @ " OR child_shape_id=" @
                %shape_id @ ";";
   sqlite.query(%query);
   
   %query = "DELETE FROM physicsShape WHERE id=" @ %shape_id @ ";";
   sqlite.query(%query);
   
   exposeMegaMotionScenesForm();   
}
///////////////////////////////////////////////////////////////////////////////

function mmSelectShape()
{
   echo("calling selectMegaMotionShape");
   
   if ($mmShapeList.getSelected()<=0)
      return;
      
   $mmShapePartList.clear();   
   %firstID = 0;
   %query = "SELECT id,name FROM physicsShapePart WHERE physicsShape_id=" @ $mmShapeList.getSelected() @ ";";  
   echo("\n" @ %query @ "\n");   
   %resultSet = sqlite.query(%query, 0); 
   if (%resultSet)
   {
      if (sqlite.numRows(%resultSet)>0)
      {
         %firstID = sqlite.getColumn(%resultSet, "id");
         while (!sqlite.endOfResult(%resultSet))
         {
            %id = sqlite.getColumn(%resultSet, "id");
            %name = sqlite.getColumn(%resultSet, "name");
            $mmShapePartList.add(%name,%id);
            sqlite.nextRow(%resultSet);         
         }
         //if (%firstID>0)
          //  $mmShapePartList.setSelected(%firstID);
      }
      sqlite.clearResult(%resultSet);
   }   

   $mmShapePartBaseNodeList.clear();
   $mmShapePartChildNodeList.clear();
   $mmSequenceAllNodeList.clear();
   $mmBvhModelNodeList.clear();
   for (%i=0;%i<$mmSelectedShape.getNumNodes();%i++)
   {
      %node_name = $mmSelectedShape.getNodeName(%i);
      $mmShapePartBaseNodeList.add(%node_name,%i);
      $mmShapePartChildNodeList.add(%node_name,%i);
      $mmSequenceAllNodeList.add(%node_name,%i);
      $mmBvhModelNodeList.add(%node_name,%i);
   }
   
   //Whoops, gotta be much more careful about not doing this on selecting scene shape, only on changing shape.
   //Finally, see if we want to change the shape of the currently selected sceneShape.
   %sceneShapeId = $mmSceneShapeList.getSelected();
   %shape_id = $mmShapeList.getSelected();
   if ((%sceneShapeId>0)&&(%shape_id!=$mmShapeId))
   {
      MessageBoxYesNo("","Really assign sceneShape " @ %sceneShapeId @ " to shape " @ 
         $mmShapeList.getText() @ "?","mmReassignShape();","");
   }
}

//Not currently hooked in, can't associate it with selectShape() above until we remove all the times
//we select the shapelist automatically, or really any time we select the shape we're already using.
function mmReassignShape()
{
   echo("REASSIGNING SHAPE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
   if (($mmSceneShapeList.getSelected()<=0)||($mmShapeList.getSelected()<=0))
      return;
      
   %sceneShapeId = $mmSceneShapeList.getSelected();
   %shape_id = $mmShapeList.getSelected();
   %query = "UPDATE sceneShape SET shape_id=" @ %shape_id @ " WHERE id=" @ %sceneShapeId @ ";";
   echo("reassigning shape: " @ %query );
   sqlite.query(%query,0);
   
   if ($mmLoadedScenes>0)
   {
      mmUnloadScene($mmSceneList.getSelected());
      mmLoadScene($mmSceneList.getSelected());
   }
   
   $mmSceneShapeList.setSelected(%sceneShapeId);
}

function mmSelectShapePart()
{
   if ($mmShapePartList.getSelected()<=0)
      return;
   
   %partId = $mmShapePartList.getSelected();
   if (%partId<=0)
      return;
	
   %tab = $mmTabBook.findObjectByInternalName("shapePartTab");
   %panel = %tab.findObjectByInternalName("shapePartPanel");
	
   %baseNodeList = %panel.findObjectByInternalName("baseNodeList");
   %childNodeList = %panel.findObjectByInternalName("childNodeList");   
   
   //Actually, let's not make these ones globals... going down to selectShapePart
   %dimensionsX = %panel.findObjectByInternalName("shapePartDimensionsX");
   %dimensionsY = %panel.findObjectByInternalName("shapePartDimensionsY");
   %dimensionsZ = %panel.findObjectByInternalName("shapePartDimensionsZ");         
   %orientationX = %panel.findObjectByInternalName("shapePartOrientationX");//eulers
   %orientationY = %panel.findObjectByInternalName("shapePartOrientationY");
   %orientationZ = %panel.findObjectByInternalName("shapePartOrientationZ");         
   %offsetX = %panel.findObjectByInternalName("shapePartOffsetX");
   %offsetY = %panel.findObjectByInternalName("shapePartOffsetY");
   %offsetZ = %panel.findObjectByInternalName("shapePartOffsetZ");   
	      
	%query = "SELECT * FROM physicsShapePart " @ 
	         "WHERE id=" @ %partId @ ";"; 
	%resultSet = sqlite.query(%query, 0);
	if (%resultSet)
	{
	   if (sqlite.numRows(%resultSet)==1)
	   {	               
         %dimensionsX.setText(sqlite.getColumn(%resultSet, "dimensions_x"));
         %dimensionsY.setText(sqlite.getColumn(%resultSet, "dimensions_y"));
         %dimensionsZ.setText(sqlite.getColumn(%resultSet, "dimensions_z"));         
         %orientationX.setText(sqlite.getColumn(%resultSet, "orientation_x"));
         %orientationY.setText(sqlite.getColumn(%resultSet, "orientation_y"));
         %orientationZ.setText(sqlite.getColumn(%resultSet, "orientation_z"));         
         %offsetX.setText(sqlite.getColumn(%resultSet, "offset_x"));
         %offsetY.setText(sqlite.getColumn(%resultSet, "offset_y"));
         %offsetZ.setText(sqlite.getColumn(%resultSet, "offset_z"));
         
         %jointId = sqlite.getColumn(%resultSet, "px3Joint_id");
         if (%jointId > 0)
            $mmJointList.setSelected(%jointId);
         
         $mmShapePartTypeList.setSelected(sqlite.getColumn(%resultSet, "shapeType"));
	   } else {
	      echo("shape part num rows: " @ sqlite.numRows(%resultSet));
	   }
	} else { 
	   echo("shape part query failed!  \n " @ %query);
	} 
}

function mmUpdateShapePart()
{
   if ($mmShapePartList.getSelected()<=0)
      return;
      
   %part_id = $mmShapePartList.getSelected();
   
   %query = "";
   
   %query = %query @ "UPDATE physicsShapePart SET "; 
   %query = %query @ "shapeType=" @ $mmShapePartTypeList.getSelected();
   
   if (strlen($mmShapePartDimensionsX.getText())>0)//(also check isNumeric)
      %query = %query @ ",dimensions_x=" @ $mmShapePartDimensionsX.getText();
   if (strlen($mmShapePartDimensionsY.getText())>0)//(also check isNumeric)
      %query = %query @ ",dimensions_y=" @ $mmShapePartDimensionsY.getText();
   if (strlen($mmShapePartDimensionsZ.getText())>0)//(also check isNumeric)
      %query = %query @ ",dimensions_z=" @ $mmShapePartDimensionsZ.getText();
   if (strlen($mmShapePartOrientationX.getText())>0)//(also check isNumeric)
      %query = %query @ ",orientation_x=" @ $mmShapePartOrientationX.getText();
   if (strlen($mmShapePartOrientationY.getText())>0)//(also check isNumeric)
      %query = %query @ ",orientation_y=" @ $mmShapePartOrientationY.getText();
   if (strlen($mmShapePartOrientationZ.getText())>0)//(also check isNumeric)
      %query = %query @ ",orientation_z=" @ $mmShapePartOrientationZ.getText();
   if (strlen($mmShapePartOffsetX.getText())>0)//(also check isNumeric)
      %query = %query @ ",offset_x=" @ $mmShapePartOffsetX.getText();
   if (strlen($mmShapePartOffsetY.getText())>0)//(also check isNumeric)
      %query = %query @ ",offset_y=" @ $mmShapePartOffsetY.getText();
   if (strlen($mmShapePartOffsetZ.getText())>0)//(also check isNumeric)
      %query = %query @ ",offset_z=" @ $mmShapePartOffsetZ.getText();
      
   %query = %query @ " WHERE id=" @ %part_id @ ";"; 
   echo("\n" @ %query @ "\n"); 
	sqlite.query(%query, 0);
}


function mmSelectJoint()
{
   %jointId = $mmJointList.getSelected();
   if (%jointId<=0)
      return;
      
   %tab = $mmTabBook.findObjectByInternalName("shapePartTab");
   %panel = %tab.findObjectByInternalName("shapePartPanel");

   %jointCount = $mmJointList.size();   
   echo("selecting joint " @ %jointId @ "!  numJoints " @ %jointCount ); 
   
   %twistLimit = %panel.findObjectByInternalName("jointTwistLimit");
   %swingLimit = %panel.findObjectByInternalName("jointSwingLimit");
   %swingLimit2 = %panel.findObjectByInternalName("jointSwingLimit2");
   %xLimit = %panel.findObjectByInternalName("jointXLimit");
   %yLimit = %panel.findObjectByInternalName("jointYLimit");
   %zLimit = %panel.findObjectByInternalName("jointZLimit");
   %axisX = %panel.findObjectByInternalName("jointAxisX");
   %axisY = %panel.findObjectByInternalName("jointAxisY");
   %axisZ = %panel.findObjectByInternalName("jointAxisZ");
   %normalX = %panel.findObjectByInternalName("jointNormalX");
   %normalY = %panel.findObjectByInternalName("jointNormalY");
   %normalZ = %panel.findObjectByInternalName("jointNormalZ");
   %twistSpring = %panel.findObjectByInternalName("jointTwistSpring");
   %swingSpring = %panel.findObjectByInternalName("jointSwingSpring");
   %springDamper = %panel.findObjectByInternalName("jointSpringDamper");
   %motorSpring = %panel.findObjectByInternalName("jointMotorSpring");
   %motorDamper = %panel.findObjectByInternalName("jointMotorDamper");
   %maxForce = %panel.findObjectByInternalName("jointMaxForce");
   %maxTorque = %panel.findObjectByInternalName("jointMaxTorque");
   
   %query = "SELECT * FROM px3Joint WHERE id=" @ %jointId @ ";";
   %resultSet = sqlite.query(%query,0);
   if (%resultSet)
	{
	   if (sqlite.numRows(%resultSet)==1)
	   {	               
         %twistLimit.setText(sqlite.getColumn(%resultSet, "twistLimit"));
         %swingLimit.setText(sqlite.getColumn(%resultSet, "swingLimit"));
         %swingLimit2.setText(sqlite.getColumn(%resultSet, "swingLimit2"));
         %xLimit.setText(sqlite.getColumn(%resultSet, "xLimit"));
         %yLimit.setText(sqlite.getColumn(%resultSet, "yLimit"));
         %zLimit.setText(sqlite.getColumn(%resultSet, "zLimit"));
         %axisX.setText(sqlite.getColumn(%resultSet, "localAxis_x"));
         %axisY.setText(sqlite.getColumn(%resultSet, "localAxis_y"));
         %axisZ.setText(sqlite.getColumn(%resultSet, "localAxis_z"));
         %normalX.setText(sqlite.getColumn(%resultSet, "localNormal_x"));
         %normalY.setText(sqlite.getColumn(%resultSet, "localNormal_y"));
         %normalZ.setText(sqlite.getColumn(%resultSet, "localNormal_z"));
         %twistSpring.setText(sqlite.getColumn(%resultSet, "twistSpring"));
         %swingSpring.setText(sqlite.getColumn(%resultSet, "swingSpring"));
         %springDamper.setText(sqlite.getColumn(%resultSet, "springDamper"));
         %motorSpring.setText(sqlite.getColumn(%resultSet, "motorSpring"));
         %motorDamper.setText(sqlite.getColumn(%resultSet, "motorDamper"));
         %maxForce.setText(sqlite.getColumn(%resultSet, "maxForce"));
         %maxTorque.setText(sqlite.getColumn(%resultSet, "maxTorque"));
         $mmJointTypeList.setSelected(sqlite.getColumn(%resultSet, "jointType"));
	   }
	}
}


//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////

function setupMMAddSceneShapeForm()
{
   
   %shapeList = mmAddSceneShapeWindow.findObjectByInternalName("shapeList"); 
   %groupList = mmAddSceneShapeWindow.findObjectByInternalName("groupList"); 
   %behaviorTree = mmAddSceneShapeWindow.findObjectByInternalName("behaviorTree"); 
   if ((!isDefined(%shapeList))||(!isDefined(%groupList)))
      return;
   
   //%shapeList
   %query = "SELECT id,name FROM physicsShape ORDER BY name;";
   %resultSet = sqlite.query(%query, 0);
   if (%resultSet)
   {
      if (sqlite.numRows(%resultSet)>0)
      {         
         //%firstID = sqlite.getColumn(%resultSet, "id");
         while (!sqlite.endOfResult(%resultSet))
         {
            %id = sqlite.getColumn(%resultSet, "id");
            %name = sqlite.getColumn(%resultSet, "name");
            
            %shapeList.add(%name,%id);
            sqlite.nextRow(%resultSet);         
         }
         //if (%firstID>0) 
            //$mmShapeList.setSelected(%firstID);
      }
      sqlite.clearResult(%resultSet);
   }
   if ($mmShapeList.getSelected()>0)
      %shapeList.setSelected($mmShapeList.getSelected());
   
   //groupList
   %query = "SELECT id,name FROM shapeGroup ORDER BY name;";
   %resultSet = sqlite.query(%query, 0);
   if (%resultSet)
   {
      if (sqlite.numRows(%resultSet)>0)
      {         
         //%firstID = sqlite.getColumn(%resultSet, "id");
         while (!sqlite.endOfResult(%resultSet))
         {
            %id = sqlite.getColumn(%resultSet, "id");
            %name = sqlite.getColumn(%resultSet, "name");
            
            %groupList.add(%name,%id);
            sqlite.nextRow(%resultSet);         
         }
         //if (%firstID>0) 
            //$mmShapeList.setSelected(%firstID);
      }
      sqlite.clearResult(%resultSet);
   }    
   if ($mmShapeGroupList.getSelected()>0)
      %groupList.setSelected($mmShapeGroupList.getSelected());
      
   if (strlen($mmSceneShapeBehaviorTree.getText())>0)
      %behaviorTree.setText($mmSceneShapeBehaviorTree.getText());
      
   %posX = mmAddSceneShapeWindow.findObjectByInternalName("shapePositionX"); 
   %posY = mmAddSceneShapeWindow.findObjectByInternalName("shapePositionY"); 
   %posZ = mmAddSceneShapeWindow.findObjectByInternalName("shapePositionZ"); 
   
   %oriX = mmAddSceneShapeWindow.findObjectByInternalName("shapeOrientationX"); 
   %oriY = mmAddSceneShapeWindow.findObjectByInternalName("shapeOrientationY"); 
   %oriZ = mmAddSceneShapeWindow.findObjectByInternalName("shapeOrientationZ"); 
   %oriAngle = mmAddSceneShapeWindow.findObjectByInternalName("shapeOrientationAngle"); 
   
   %scaleX = mmAddSceneShapeWindow.findObjectByInternalName("shapeScaleX"); 
   %scaleY = mmAddSceneShapeWindow.findObjectByInternalName("shapeScaleY"); 
   %scaleZ = mmAddSceneShapeWindow.findObjectByInternalName("shapeScaleZ"); 
   
   %posX.setText(0);
   %posY.setText(0);
   %posZ.setText(0);
   
   %oriX.setText(0);
   %oriY.setText(0);
   %oriZ.setText(1);
   %oriAngle.setText(0);
   
   %scaleX.setText(1);
   %scaleY.setText(1);
   %scaleZ.setText(1);
     
   %blockX = mmAddSceneShapeWindow.findObjectByInternalName("blockCountX"); 
   %blockY = mmAddSceneShapeWindow.findObjectByInternalName("blockCountY"); 
   %blockPaddingX = mmAddSceneShapeWindow.findObjectByInternalName("blockPaddingX"); 
   %blockPaddingY = mmAddSceneShapeWindow.findObjectByInternalName("blockPaddingY"); 
   %blockVariationX = mmAddSceneShapeWindow.findObjectByInternalName("blockVariationX"); 
   %blockVariationY = mmAddSceneShapeWindow.findObjectByInternalName("blockVariationY"); 
   %blockRotX = mmAddSceneShapeWindow.findObjectByInternalName("blockRotationX"); 
   %blockRotY = mmAddSceneShapeWindow.findObjectByInternalName("blockRotationY"); 
   %blockRotZ = mmAddSceneShapeWindow.findObjectByInternalName("blockRotationZ"); 
   %blockRotAngle = mmAddSceneShapeWindow.findObjectByInternalName("blockRotationAngle"); 
   
   %blockX.setText(2);
   %blockY.setText(2);
   %blockPaddingX.setText(2);
   %blockPaddingY.setText(2);
   %blockVariationX.setText(0);
   %blockVariationY.setText(0);
   %blockRotX.setText(0);
   %blockRotY.setText(0);
   %blockRotZ.setText(1);
   %blockRotAngle.setText(0);
   
}

function mmReallyAddSceneShape() //TO DO: pos/rot/scale, shapeGroup, behaviorTree.
{
   %name = mmAddSceneShapeWindow.findObjectByInternalName("nameEdit").getText(); 
   //if (substr(%name," ")>0)
   //{
   //   MessageBoxOK("Name Invalid","Scene name must be a unique character string with no spaces or special characters.","");
   //   return;  
   //}
   %scene_id = $mmSceneList.getSelected();
   %shape_id = mmAddSceneShapeWindow.findObjectByInternalName("shapeList").getSelected();
   %group_id = mmAddSceneShapeWindow.findObjectByInternalName("groupList").getSelected();
   %behavior_tree = mmAddSceneShapeWindow.findObjectByInternalName("behaviorTree").getText();
   
   %pos_x = mmAddSceneShapeWindow.findObjectByInternalName("shapePositionX").getText();
   %pos_y = mmAddSceneShapeWindow.findObjectByInternalName("shapePositionY").getText();
   %pos_z = mmAddSceneShapeWindow.findObjectByInternalName("shapePositionZ").getText(); 
   
   %ori_x = mmAddSceneShapeWindow.findObjectByInternalName("shapeOrientationX").getText();
   %ori_y = mmAddSceneShapeWindow.findObjectByInternalName("shapeOrientationY").getText();
   %ori_z = mmAddSceneShapeWindow.findObjectByInternalName("shapeOrientationZ").getText(); 
   %ori_a = mmAddSceneShapeWindow.findObjectByInternalName("shapeOrientationAngle").getText();
   
   %scale_x = mmAddSceneShapeWindow.findObjectByInternalName("shapeScaleX").getText();
   %scale_y = mmAddSceneShapeWindow.findObjectByInternalName("shapeScaleY").getText();
   %scale_z = mmAddSceneShapeWindow.findObjectByInternalName("shapeScaleZ").getText(); 
   
   if (strlen(%name)>0)
   {
      %query = "SELECT id FROM sceneShape WHERE name='" @ %name @ "' AND scene_id=" @ %scene_id @ ";";
      %resultSet = sqlite.query(%query,0);
      if (sqlite.numRows(%resultSet)>0)
      {
         MessageBoxOK("Name Invalid","Scene shape name must be unique for this scene.","");
         return;
      }
   }
   
   //
   //HERE: do some sanity testing before we commit!
   //
   
   //And, insert pos, rot & scale, and get the ids back. Q: what is the most efficient way to do this?
   //For now, I'm inserting the other stuff, grabbing an id, and then inserting the pos/rot/scale and
   //using last_insert_rowid() in update statements.
   %query = "INSERT INTO sceneShape (name,scene_id,shape_id,shapeGroup_id,behavior_tree) " @
            " VALUES ('" @ %name @ "'," @ %scene_id @ "," @ %shape_id @ "," @ %group_id @
             ",'" @ %behavior_tree @ "');";
   sqlite.query(%query,0);

   //These are optional, check for values first.      
   //,shapeGroup_id,behaviorTree
   // "," @ %group_id @ ",'" @ %behaviorTree @ "'"

   %ssID = 0;
   %query = "SELECT id FROM sceneShape WHERE name='" @ %name @ "' AND scene_id=" @ %scene_id @ ";";
   %resultSet = sqlite.query(%query,0);
   if (sqlite.numRows(%resultSet)==1)
   {
      %ss_id = sqlite.getColumn(%resultSet, "id");
      sqlite.clearResult(%resultSet);
   }
   if (%ss_id==0)
      return;
   
   //For first pass at least, just add default values and spawn the character at scene origin.
   %query = "INSERT INTO vector3 (x,y,z) VALUES (" @ %pos_x @ "," @ %pos_y @ "," @ %pos_z @ ");";
   sqlite.query(%query,0);
   %query = "UPDATE sceneShape SET pos_id=last_insert_rowid() WHERE id=" @ %ss_id @ ";";
   sqlite.query(%query, 0);  
         
   %query = "INSERT INTO rotation (x,y,z,angle) VALUES (" @ %ori_x @ "," @ %ori_y @ "," @ 
                  %ori_z @  "," @ %ori_a @ ");";      
   sqlite.query(%query,0);
   %query = "UPDATE sceneShape SET rot_id=last_insert_rowid() WHERE id=" @ %ss_id @ ";";
   sqlite.query(%query, 0);  
   
   %query = "INSERT INTO vector3 (x,y,z) VALUES (" @ %scale_x @ "," @ %scale_y @ "," @ %scale_z @ ");";      
   sqlite.query(%query,0);
   %query = "UPDATE sceneShape SET scale_id=last_insert_rowid() WHERE id=" @ %ss_id @ ";";
   sqlite.query(%query, 0);  
   
   mmAddSceneShapeWindow.delete();
      
   exposeMegaMotionScenesForm();
   
   if ($mmLoadedScenes>0)
   {
      mmUnloadScene($mmSceneList.getSelected());
      mmLoadScene($mmSceneList.getSelected());
   }
}



//////////////////////////////////////////////////////////////////////


function mmAddShapeGroup() 
{
   makeSqlGuiForm($mmAddShapeGroupWindowID);
}

function mmReallyAddShapeGroup() 
{
   if (mmAddShapeGroupWindow.isVisible())
   {
      %name = mmAddShapeGroupWindow.findObjectByInternalName("nameEdit").getText(); 
      if (strlen(%name)==0)//TEST FOR UNIQUE
      {
         MessageBoxOK("Name Invalid","Group name must exist!","");
         return;  
      }
      %query = "SELECT id FROM shapeGroup WHERE name='" @ %name @ "';";
      %resultSet = sqlite.query(%query,0);
      if (sqlite.numRows(%resultSet)>0)
      {
         MessageBoxOK("Name Invalid","Group name must be unique.","");
         return;
      }
      %query = "INSERT INTO shapeGroup (name) VALUES ('" @ %name @ "');";
      sqlite.query(%query,0);
      
      mmAddShapeGroupWindow.delete();
      
      exposeMegaMotionScenesForm();
   }
}

function mmDeleteShapeGroup()
{
   //nothing here yet  
}

function mmAddSceneShapeBlock() 
{   
   %scene_id = $mmSceneList.getSelected();
   %shape_id = mmAddSceneShapeWindow.findObjectByInternalName("shapeList").getSelected();

   %groupList = mmAddSceneShapeWindow.findObjectByInternalName("groupList");
   %group_id = %groupList.getSelected();
   %group_name = %groupList.getText();
   
   if ((%scene_id<=0)||(%shape_id<=0)||(%group_id<=0))
   {
      echo("Please select a scene, shape, and group before creating a block.");
      return;
   }
   
   echo("calling addSceneShapeBlock, clock  " @ getClock() );
   %lastClock = getClock();
   //AND... new way! In engine, SQL queries go much faster.
   addSceneShapeBlock(%scene_id,%shape_id,%group_id);

   echo("finished adding characters, clock  " @ getClock() @ " elapsed " @ getClock()-%lastClock);
   %lastClock = getClock();
   
   if ($mmLoadedScenes>0)
   {
      mmUnloadScene($mmSceneList.getSelected());
      mmLoadScene($mmSceneList.getSelected());
   }
  
   mmAddShapeGroupWindow.delete();
      
   exposeMegaMotionScenesForm();
}

//////////////////////////////////////////////////////////////


function mmAddOpenSteer()
{
   makeSqlGuiForm($mmAddOpenSteerWindowID);
}

function mmReallyAddOpenSteer()
{
   if (mmAddOpenSteerWindow.isVisible())
   {
      %name = mmAddOpenSteerWindow.findObjectByInternalName("nameEdit").getText(); 
      
      %query = "INSERT INTO openSteerProfile (name) VALUES ('" @ %name @ "');";
      sqlite.query(%query,0);
      
      mmAddOpenSteerWindow.delete();
      
      exposeMegaMotionScenesForm();
   }
}

function mmDeleteOpenSteer()
{
   %openSteer_id = $mmOpenSteerList.getSelected();
   if (%openSteer_id<=0)
      return;
      
   MessageBoxOKCancel( "Warning", 
      "This will permanently delete this OpenSteer Profile! Are you completely sure?", 
      "mmReallyDeleteOpenSteer();",
      "" ); 
      
}
   
function mmReallyDeleteOpenSteer()
{
   %openSteer_id = $mmOpenSteerList.getSelected();
      
   %query = "DELETE FROM openSteerProfile WHERE id=" @ %openSteer_id @ ";";
   sqlite.query(%query,0);
   
   exposeMegaMotionScenesForm();
}


function mmSelectOpenSteer()
{   
   %openSteerID = $mmOpenSteerList.getSelected();
   if (%openSteerID<=0)
      return;
      
   %tab = $mmTabBook.findObjectByInternalName("sceneShapeTab");
   %panel = %tab.findObjectByInternalName("sceneShapePanel");
   
   %mass = %panel.findObjectByInternalName("sceneShapeOpenSteerMass");
   %radius = %panel.findObjectByInternalName("sceneShapeOpenSteerRadius");
   %maxForce = %panel.findObjectByInternalName("sceneShapeOpenSteerMaxForce");
   %maxSpeed = %panel.findObjectByInternalName("sceneShapeOpenSteerMaxSpeed");
   %wanderChance = %panel.findObjectByInternalName("sceneShapeOpenSteerWanderChance");
   %wanderWeight = %panel.findObjectByInternalName("sceneShapeOpenSteerWanderWeight");
   %seekTarget = %panel.findObjectByInternalName("sceneShapeOpenSteerSeekTarget");
   %avoidTarget = %panel.findObjectByInternalName("sceneShapeOpenSteerAvoidTarget");
   %seekNeighbor = %panel.findObjectByInternalName("sceneShapeOpenSteerSeekNeighbor");
   %avoidNeighbor = %panel.findObjectByInternalName("sceneShapeOpenSteerAvoidNeighbor");
   %avoidEdge = %panel.findObjectByInternalName("sceneShapeOpenSteerAvoidEdge");
   %detectEdge = %panel.findObjectByInternalName("sceneShapeOpenSteerDetectEdge");
   
   %query = "SELECT * FROM openSteerProfile WHERE id=" @ %openSteerID @ ";";
   %resultSet = sqlite.query(%query,0);
   
   if (%resultSet)
   {
      %mass.setText(sqlite.getColumn(%resultSet,"mass"));
      %radius.setText(sqlite.getColumn(%resultSet,"radius"));
      %maxForce.setText(sqlite.getColumn(%resultSet,"maxForce"));
      %maxSpeed.setText(sqlite.getColumn(%resultSet,"maxSpeed"));
      %wanderChance.setText(sqlite.getColumn(%resultSet,"wanderChance"));
      %wanderWeight.setText(sqlite.getColumn(%resultSet,"wanderWeight"));
      %seekTarget.setText(sqlite.getColumn(%resultSet,"seekTargetWeight"));
      %avoidTarget.setText(sqlite.getColumn(%resultSet,"avoidTargetWeight"));
      %seekNeighbor.setText(sqlite.getColumn(%resultSet,"seekNeighborWeight"));
      %avoidNeighbor.setText(sqlite.getColumn(%resultSet,"avoidNeighborWeight"));
      %avoidEdge.setText(sqlite.getColumn(%resultSet,"avoidNavMeshEdgeWeight"));
      %detectEdge.setText(sqlite.getColumn(%resultSet,"detectNavMeshEdgeRange")); 
   }
}



//////////////////////////////////////////////////////////////////////////////////
//Sequence Tab

function mmSelectSequence()
{
   echo("selecting sequence! " @ $mmSequenceList.getSelected());
   
   $mmSequenceNodeList.clear();    
   $mmSequenceKeyframeSeriesList.clear();  
   $mmSequenceKeyframeList.clear();
   
   if (($mmSequenceList.getSelected()<=0)||($mmSelectedShape<=0) || (!isObject($mmSelectedShape)))
   {
      $mmSequenceFileText.setText("");
      return;
   }
   
   %seq_id = $mmSequenceList.getSelected();
   $mmSequenceFileText.setText($mmSelectedShape.getSeqFilename(%seq_id));
   
   $mmSequenceBlend = $mmSelectedShape.getSeqBlend(%seq_id);
   $mmSequenceCyclic = $mmSelectedShape.getSeqCyclic(%seq_id);
   
   $mmSelectedShape.playSeqByNum($mmSequenceList.getSelected());
   $mmSelectedShape.pauseSeq();
   $mmSelectedShape.setSeqPos(0);
   
     
   %numNodes = $mmSelectedShape.getNumMattersNodes(%seq_id);
   for (%i=0;%i<%numNodes;%i++)
   {
      //echo("sequence " @ %seq_id @ " nodes, mattersNode " @ $mmSelectedShape.getMattersNodeIndex(%seq_id,%i) @
      //       " nodeMatters " @ $mmSelectedShape.getNodeMattersIndex(%seq_id,%i)); 
      %node_index = $mmSelectedShape.getMattersNodeIndex(%seq_id,%i);
      %node_name = $mmSelectedShape.getNodeName(%node_index);
      $mmSequenceNodeList.add(%node_name,%node_index);
   }
   
   %typenames[0]="adjust_pos";
   %typenames[1]="set_pos";
   %typenames[2]="adjust_rot";
   %typenames[3]="set_rot";
   
   $mmSequenceKeyframeSeriesList.add("",0);   
   %firstID = 0;
   %query = "SELECT id,type,node FROM keyframeSeries WHERE set_id IN" @
            " (SELECT id FROM keyframeSet" @ 
            " WHERE shape_id=" @ $mmSelectedShape.shapeID @ 
            " AND sequence_name='" @ $mmSequenceList.getText() @ "');"; 
   %resultSet = sqlite.query(%query, 0); 
   if (%resultSet)
   {
      if (sqlite.numRows(%resultSet)>0)
      {
         %firstID = sqlite.getColumn(%resultSet, "id");
         while (!sqlite.endOfResult(%resultSet))
         {
            %id = sqlite.getColumn(%resultSet, "id");
            %type = sqlite.getColumn(%resultSet, "type");
            %typename = %typenames[%type];
            %node = sqlite.getColumn(%resultSet, "node"); 
            %nodename =  $mmSelectedShape.getNodeName(%node);    
            %name =  %nodename @ " - " @ %typename @ " " @ %id;
            $mmSequenceKeyframeSeriesList.add(%name,%id);
            sqlite.nextRow(%resultSet);         
         }
         //if (%firstID>0) 
         //   $mmSequenceKeyframeSeriesList.setSelected(%firstID);
      }
      sqlite.clearResult(%resultSet);
   }
   
   $mmSequenceKeyframeList.clear();
   %firstID = 0;
   %query = "SELECT id,frame,x,y,z FROM keyframe WHERE series_id=" @
            $mmSequenceKeyframeSeriesList.getSelected() @ ";";
   %resultSet = sqlite.query(%query, 0); 
   
   if (%resultSet)
   {
      if (sqlite.numRows(%resultSet)>0)
      {
         %firstID = sqlite.getColumn(%resultSet, "id");
         while (!sqlite.endOfResult(%resultSet))
         {
            %id = sqlite.getColumn(%resultSet, "id");
            %frame = sqlite.getColumn(%resultSet, "frame");
            %x = sqlite.getColumn(%resultSet, "x");
            %y = sqlite.getColumn(%resultSet, "y");
            %z = sqlite.getColumn(%resultSet, "z");              
            %name = "frame " @  %frame ;// @ " value " @ %x @ " " @ %y @ " " @ %z  ;
            $mmSequenceKeyframeList.add(%name,%id);
            sqlite.nextRow(%resultSet);         
         }
         //if (%firstID>0) 
          //  $mmSequenceKeyframeList.setSelected(%firstID);
      }
      sqlite.clearResult(%resultSet);
   }   
   
   if (MegaMotionSequenceWindow.isVisible())
   {
      %frames = $mmSelectedShape.getSeqFrames(%seq_id);
      $mmSequenceSlider.range = "0 " @ %frames;
      $mmSequenceSlider.value = 0;
      $mmSequenceOutFrame.setText(%frames);
   }
   echo("ended selectSequence!");
   
}

/*
////(FROM EM) //////0
      
toolsWindow::selectSequence()
{
   ...
   
      if ($actor.getSeqNumKeyframes(%seqnum) == $actor.getSeqNumGroundFrames(%seqnum))
      {
         $sequence_ground_animate = true;
         groundCaptureSeqButton.setText("Un Ground Capture");
         GroundAnimateCheckbox.visible = true;
      } else {
         $sequence_ground_animate = false;
         groundCaptureSeqButton.setText("Ground Capture");
         GroundAnimateCheckbox.visible = false;
      }

   ...     
}
      
function EcstasyToolsWindow::toggleGroundAnimate()
{
   if (EWorldEditor.getSelectionSize()>0)
   {
      for (%i=0;%i<EWorldEditor.getSelectionSize();%i++)
      {
         %obj = EWorldEditor.getSelectedObject( %i );
         if (%obj)
         {
           if (%obj.getClassName() $= "fxFlexBody")
           {
              //echo("playing sequence " @ SequencesList.getText() @ " on actor " @ %obj.gatActorID());
               %ghostID = LocalClientConnection.getGhostID(%obj);
               %client_bot = ServerConnection.resolveGhostID(%ghostID);
               %client_bot.setGroundAnimating($tweaker_ground_animate);           
           }
         }
      }
   } 
   else if ($actor) 
   {
      $actor.setGroundAnimating($tweaker_ground_animate);
   }
}



*/

   //Nice thought, but nope:
   //%query = "SELECT id,frame,x,y,z FROM keyframe WHERE series_id IN" @ 
   //         " (SELECT id FROM keyframeSeries WHERE set_id IN" @ 
   //         " (SELECT id FROM keyframeSet" @ 
   //         " WHERE shape_id=" @ $mmSelectedShape.shapeID @ 
   //         " AND sequence_name='" @ $mmSequenceList.getText() @ "'));";  
   
function mmAddSequence()
{
   if (!isObject($mmSelectedShape))
      return;
         
   if (strlen($Pref::DsqDir))
      %openFileName = mmGetOpenFilename($Pref::DsqDir,"dsq");
   else
      %openFileName = mmGetOpenFilename($mmSelectedShape.getPath(),"dsq");
   
   if (strlen(%openFileName))
   {     
      %openFileName = strreplace(%openFileName,"'","''");//Escape single quotes in the name.
      if (!$mmSelectedShape.loadSequence(%openFileName))
         return;   	  
   } else return;

   mmSelectShape();

   $mmSequenceList.setSelected($mmSequenceList.size()-1);
   
   return;
}

function mmAddSequenceByFilename(%filename)
{   
   if (!isObject($mmSelectedShape))
      return;      
   
   if (strlen(%filename))
   {       
     %filename = strreplace(%filename,"'","''");//Escape single quotes in the name.
      if (!$mmSelectedShape.loadSequence(%filename))
         return;   //possible repetition of addSequence taking place now...	  
   } else return;

   mmSelectShape();

   $mmSequenceList.setSelected($mmSequenceList.size()-1);
   
   return;   
}

function mmDropSequence()
{
   if (!isObject($mmSelectedShape))
      return;     
       
   $mmSelectedShape.dropSequence($mmSequenceList.getSelected());
    
}
   
function mmSaveSequence()
{
   if (!isObject($mmSelectedShape))
      return;      
       
   if (strlen($Pref::DsqDir))
      %saveFileName = mmGetSaveFileName($Pref::DsqDir,"dsq");
   else
      %saveFileName = mmGetSaveFileName($actor.getPath(),"dsq");
         
   $mmSelectedShape.saveSequence($mmSequenceList.getSelected(),%saveFileName);
   
 
}
   
function mmAddSceneSequence()
{
   
}
   
function mmSelectSequenceNode()
{   
   %typenames[0]="adjust_pos";
   %typenames[1]="set_pos";
   %typenames[2]="adjust_rot";
   %typenames[3]="set_rot";
   
   $mmSequenceKeyframeSeriesList.clear();   
   %firstID = 0;
   %node_id = $mmSequenceNodeList.getSelected();
   %query = "SELECT id,type FROM keyframeSeries WHERE node=" @ %node_id @ 
            " AND set_id IN (SELECT id FROM keyframeSet" @ 
            " WHERE shape_id=" @ $mmSelectedShape.shapeID @ 
            " AND sequence_name='" @ $mmSequenceList.getText() @ "');"; 
   %resultSet = sqlite.query(%query, 0); 
   if (%resultSet)
   {
      if (sqlite.numRows(%resultSet)>0)
      {
         %firstID = sqlite.getColumn(%resultSet, "id");
         while (!sqlite.endOfResult(%resultSet))
         {
            %id = sqlite.getColumn(%resultSet, "id");
            %type = sqlite.getColumn(%resultSet, "type");
            %typename = %typenames[%type];
            %nodename =  $mmSelectedShape.getNodeName(%node_id);    
            %name =  %nodename @ " - " @ %typename @ " " @ %id;
            $mmSequenceKeyframeSeriesList.add(%name,%id);
            sqlite.nextRow(%resultSet);         
         }
         if (%firstID>0) 
            $mmSequenceKeyframeSeriesList.setSelected(%firstID);
      } 
      else 
      {
         $mmSequenceKeyframeSeriesList.clear();
         $mmSequenceKeyframeList.clear();     
         $mmKeyframeID = 0;    
      }
      sqlite.clearResult(%resultSet);
   }
}

function mmSelectKeyframeSeries()
{
   
   $mmSequenceKeyframeList.clear();
   %firstID = 0;
   %query = "SELECT id,frame,x,y,z FROM keyframe WHERE series_id=" @
            $mmSequenceKeyframeSeriesList.getSelected() @ ";";
   %resultSet = sqlite.query(%query, 0); 
   
   if (%resultSet)
   {
      if (sqlite.numRows(%resultSet)>0)
      {
         %firstID = sqlite.getColumn(%resultSet, "id");
         while (!sqlite.endOfResult(%resultSet))
         {
            %id = sqlite.getColumn(%resultSet, "id");
            %frame = sqlite.getColumn(%resultSet, "frame");
            %x = sqlite.getColumn(%resultSet, "x");
            %y = sqlite.getColumn(%resultSet, "y");
            %z = sqlite.getColumn(%resultSet, "z");              
            %name = "frame " @  %frame ; // @ " value " @ %x @ " " @ %y @ " " @ %z  ;
            $mmSequenceKeyframeList.add(%name,%id);
            sqlite.nextRow(%resultSet);         
         }
         if (%firstID>0) 
            $mmSequenceKeyframeList.setSelected(%firstID);
      }
      sqlite.clearResult(%resultSet);
   }   
   
   
}

function mmSelectKeyframe()
{
   if (!isObject($mmSelectedShape))
      return;     
   
   %panel = $mmSequenceTab.findObjectByInternalName("sequencePanel");
   %sumX = %panel.findObjectByInternalName("sequenceKeyframeSumX");
   %sumY = %panel.findObjectByInternalName("sequenceKeyframeSumY");
   %sumZ = %panel.findObjectByInternalName("sequenceKeyframeSumZ");
   %frame = %panel.findObjectByInternalName("sequenceKeyframeFrame");
   
   $mmKeyframeID = $mmSequenceKeyframeList.getSelected();
   %query = "SELECT id,frame,x,y,z FROM keyframe WHERE id=" @ $mmKeyframeID @ ";";
   %resultSet = sqlite.query(%query, 0); 
   if (%resultSet)
   {
      %frame = sqlite.getColumn(%resultSet, "frame");
      %x = sqlite.getColumn(%resultSet, "x");
      %y = sqlite.getColumn(%resultSet, "y");
      %z = sqlite.getColumn(%resultSet, "z"); 
       
      %sumX.setText(%x);
      %sumY.setText(%y);
      %sumZ.setText(%z);  
      
      %frame.setText(%frame);      
      $mmSequenceSlider.setValue(%frame);
      $mmSelectedShape.setSeqPos(%frame/$mmSequenceSlider.range.y);
      $mmSelectedShape.pauseSeq();
   }   
}

function mmAddSequenceAction()
{
   //nothing here yet   
}

function mmDeleteSequenceAction()
{
   //nothing here yet   
}

function mmAssignSequenceAction()
{   
   %profile_id = $mmSelectedShape.actionProfileID;
   %seqFile = $mmSequenceFileText.getValue();
   %action_id = $mmSequenceActionList.getSelected();
   
   if ((%profile_id<=0)||(strlen(%seqFile)==0)||(%action_id<=0))
      return;
   
   %query = "SELECT id FROM actionProfileSequence WHERE " @
            "profile_id=" @ %profile_id @ " AND action_id=" @ 
            %action_id @ ";";
   %resultSet = sqlite.query(%query,0);
   if (%resultSet)
   {
      %actionSeq_id = sqlite.getColumn(%resultSet,"id");
      %query = "UPDATE actionProfileSequence SET sequence_name='" @ 
               %seqFile @ "' WHERE id=" @ %actionSeq_id @ ";";
      sqlite.query(%query,0);
   }
   else
   {
      %query = "INSERT INTO actionProfileSequence (profile_id,action_id,sequence_name) " @
               "VALUES (" @ %profile_id @ "," @ %action_id @ ",'" @ %seqFile @ "');";
      sqlite.query(%query,0);      
   }   
}

function mmUnassignSequenceAction()
{
   echo("mmUnassignSequenceAction is currently unavailable.");   
}

function mmAddMattersNode()
{
   if (!isObject($mmSelectedShape))
      return;     
      
   $mmSelectedShape.addMattersNode($mmSequenceList.getSelected(),$mmSequenceAllNodeList.getSelected());
   
   exposeMegaMotionScenesForm();
}

function mmDropMattersNode()
{
   if (!isObject($mmSelectedShape))
      return;   
  
   $mmSelectedShape.dropMattersNode($mmSequenceList.getSelected(),$mmSequenceAllNodeList.getSelected());
   
   exposeMegaMotionScenesForm();
}

//ultraframe types: 0=ADJUST_NODE_POS, 1=SET_NODE_POS, 2=ADJUST_NODE_ROT, 3=SET_NODE_ROT
function mmAddKeyframe()//This is actually addAdjustKeyframe, it's the + button. Need an addSetKeyframe function,
{ //or else call this one from both with an argument (much better).
   if (!isObject($mmSelectedShape)||($mmSelectedShape.shapeID<=0)||($mmSequenceList.getSelected()<=0))
      return;
      
   //First check the slider frame, see if we're sitting on a keyframe via db query, and if so then
   //update that one, else make a new one. But wait, if we hit + it's because we explicitly want a new one.
   //For now let's just focus on making new ones. Assume XYZ values and node list are loaded for us, use them.
   %shape_id = $mmSelectedShape.shapeID;
   %series_id = $mmSequenceKeyframeSeriesList.getSelected();
   %node = $mmSequenceNodeList.getSelected();
   %frame = mFloor($mmSequenceSlider.getValue());
   %x = $mmSequenceKeyframeValueX.getText();
   %y = $mmSequenceKeyframeValueY.getText();
   %z = $mmSequenceKeyframeValueZ.getText(); 
    
   %type = 2;
   if ($mmKeyframesRotation)
      %type = 2;
   else //if ($mmKeyframesPosition)
      %type = 0;
   
   if (%series_id>0)
   { 
      %query = "INSERT INTO keyframe (series_id,frame,x,y,z) VALUES (" @ %series_id @ "," @
               %frame @ "," @ %x @ "," @ %y @ "," @ %z @ ");";
      sqlite.query(%query,0);
   } else {
      %query = "SELECT * FROM keyframeSet WHERE shape_id=" @ %shape_id @ 
               " AND sequence_name='" @ $mmSequenceList.getText() @ "';";
      %resultSet = sqlite.query(%query,0);
      if (sqlite.numRows(%resultSet)==1)
      {
         %set_id = sqlite.getColumn(%resultSet,"id");
         sqlite.clearResult(%resultSet);
      }
      if (%set_id>0)
      { 
         %query = "INSERT INTO keyframeSeries (set_id,type,node) VALUES (" @ %set_id @ "," @
                  %type @ "," @ %node @ ");";
         sqlite.query(%query,0);      
      
         %query = "INSERT INTO keyframe (series_id,frame,x,y,z) VALUES (last_insert_rowid()" @ 
                  "," @ %frame @ "," @ %x @ "," @ %y @ "," @ %z @ ");";
         sqlite.query(%query,0);
      } 
      else 
      {      
         %query = "INSERT INTO keyframeSet (shape_id,sequence_name) VALUES (" @ %shape_id @ ",'" @
               $mmSequenceList.getText() @ "');";
         sqlite.query(%query,0);
         
         %query = "INSERT INTO keyframeSeries (set_id,type,node) VALUES (last_insert_rowid()," @  
                   %type @ "," @ %node @ ");";
         sqlite.query(%query,0);
         
         %query = "INSERT INTO keyframe (series_id,frame,x,y,z) VALUES (last_insert_rowid()" @ 
                  "," @ %frame @ "," @ %x @ "," @ %y @ "," @ %z @ ");";
         sqlite.query(%query,0);  
      }
   }   
   exposeMegaMotionScenesForm();
}

function mmDeleteKeyframe()
{
   %shape_id = $mmSelectedShape.shapeID;
   %series_id = $mmSequenceKeyframeSeriesList.getSelected();
   %keyframe_id = $mmSequenceKeyframeList.getSelected();
   if (%keyframe_id>0)
   {
      %query = "DELETE FROM keyframe WHERE id=" @ %keyframe_id @ ";";
      sqlite.query(%query,0);
      
      //See if this series has other keyframes, if so we're done. If not delete it, and possibly the set.
      %query = "SELECT id FROM keyframe WHERE series_id=" @ %series_id @ ";";
      %resultSet = sqlite.query(%query,0);
      if (sqlite.numRows(%resultSet)>0)
         return; //didn't clear result before returning... but is it really necessary? 
      sqlite.clearResult(%resultSet);
      
      //Now, see if there are any other keyframeSeries sharing this set_id, and if not, delete the set.
      //FIX: we should really start doing this on the SQLite side with foreign keys and cascading deletes.
      %query = "SELECT set_id FROM keyframeSeries WHERE id=" @ %series_id @ ";";
      %resultSet = sqlite.query(%query,0);
      %set_id = sqlite.getColumn(%resultSet,"set_id");            
      sqlite.clearResult(%resultSet);
      
      %query = "SELECT id FROM keyframeSeries WHERE set_id=" @ %set_id @ " AND id!=" @ %series_id @ ";";
      %resultSet2 = sqlite.query(%query,0);  
      
      %query = "DELETE FROM keyframeSeries WHERE id=" @ %series_id @ ";";
      sqlite.query(%query);
      if (sqlite.numRows(%resultSet2)>0)
      {
         echo("Found more keyframeSeries in this set, not deleting it.");
         return;
      }         
      sqlite.clearResult(%resultSet2);

      %query = "DELETE FROM keyframeSet WHERE id=" @ %set_id @ ";";
      sqlite.query(%query,0);
   }   
   //exposeMegaMotionScenesForm();
}

//Maybe unnecessary? Adding/deleting keyframe series may just be handled automatically,
// by adding/deleting keyframes.
function mmAddKeyframeSeries()
{   
   //nothing here yet
}

function mmDeleteKeyframeSeries()
{   
   //nothing here yet
}

///////////////////////////////////////////////////////////////////
// +/-/Set : search for a keyframe on the current frame for the current series. If found update it, if
//not found either create a new one on the fly, or call up a gui and ask nicely. (Decide after testing.)
//If doing it on the fly, allow unselecting all series on the series list, and then when you hit 
//the +/-/set buttons it will automatically make a new series, starting with a single frame.
 
function mmSetNode()
{
   %keyframe_id = $mmSequenceKeyframeList.getSelected();
   if (%keyframe_id<=0)
      return;
   
   %x = $mmSequenceKeyframeValueX.getText();
   %y = $mmSequenceKeyframeValueY.getText();
   %z = $mmSequenceKeyframeValueZ.getText();
   if (strlen(%x)==0) %x=0;
   if (strlen(%y)==0) %y=0;
   if (strlen(%z)==0) %z=0;
   
      
   %query = "UPDATE keyframe SET x=" @ %x @ ",y=" @ %y @ ",z=" @ %z @ " WHERE id=" @ %keyframe_id @ ";";
   sqlite.query(%query,0);
   
   mmLoadKeyframeSets();
   mmSelectKeyframe();    
}

function mmAdjustNode()
{
   %keyframe_id = $mmSequenceKeyframeList.getSelected();
   if (%keyframe_id<=0)
      return;
   
   %query = "SELECT x,y,z FROM keyframe WHERE id=" @ %keyframe_id @ ";";
   %resultSet = sqlite.query(%query,0);
   %sumX = %sumY = %sumZ = 0;
   if (sqlite.numRows(%resultSet)==1)
   {
      %sumX = sqlite.getColumn(%resultSet,"x");
      %sumY = sqlite.getColumn(%resultSet,"y");
      %sumZ = sqlite.getColumn(%resultSet,"z");
      sqlite.clearResult(%resultSet);
   }   
   
   %x = $mmSequenceKeyframeValueX.getText();
   %y = $mmSequenceKeyframeValueY.getText();
   %z = $mmSequenceKeyframeValueZ.getText();
   
   if (strlen(%x)==0) %x=0;
   if (strlen(%y)==0) %y=0;
   if (strlen(%z)==0) %z=0;
         
   %query = "UPDATE keyframe SET x=" @ %sumX + %x @ ",y=" @ %sumY + %y @ ",z=" @ %sumZ + %z @ 
            " WHERE id=" @ %keyframe_id @ ";";
   sqlite.query(%query,0);
   
   mmLoadKeyframeSets();   
   mmSelectKeyframe(); 
}

function mmUnadjustNode()
{
   %keyframe_id = $mmSequenceKeyframeList.getSelected();
   if (%keyframe_id<=0)
      return;
   
   %query = "SELECT x,y,z FROM keyframe WHERE id=" @ %keyframe_id @ ";";
   %resultSet = sqlite.query(%query,0);
   %sumX = %sumY = %sumZ = 0;
   if (sqlite.numRows(%resultSet)==1)
   {
      %sumX = sqlite.getColumn(%resultSet,"x");
      %sumY = sqlite.getColumn(%resultSet,"y");
      %sumZ = sqlite.getColumn(%resultSet,"z");
      sqlite.clearResult(%resultSet);
   }   
   
   %x = $mmSequenceKeyframeValueX.getText();
   %y = $mmSequenceKeyframeValueY.getText();
   %z = $mmSequenceKeyframeValueZ.getText();
   
   if (strlen(%x)==0) %x=0;
   if (strlen(%y)==0) %y=0;
   if (strlen(%z)==0) %z=0;
   
   %x *= -1;
   %y *= -1; 
   %z *= -1;
       
   %query = "UPDATE keyframe SET x=" @ %sumX + %x @ ",y=" @ %sumY + %y @ ",z=" @ %sumZ + %z @ 
            " WHERE id=" @ %keyframe_id @ ";";
   sqlite.query(%query,0);
   
   mmLoadKeyframeSets();     
   mmSelectKeyframe(); 
   
}

function mmStartCentered()
{
   if (!isObject($mmSelectedShape))
      return;
      
   %seq = $mmSequenceList.getSelected();
   if (%seq > -1) 
   {  
      %initialPos = $mmSelectedShape.getNodeTrans(%seq,0);//returns frame zero
      %deltaX = -1 * getWord(%initialPos,0);
      %deltaY = -1 * getWord(%initialPos,1);
      %deltaZ = 0;
      %deltaPos = %deltaX @ " " @ %deltaY @ " " @ %deltaZ;
      $mmSelectedShape.adjustBaseNodePosRegion(%seq,%deltaPos,0.0,1.0);
   }
}

function mmFaceForward()
{
   if (!isObject($mmSelectedShape))
      return;
      
   %seq = $mmSequenceList.getSelected();
   if (%seq > -1)
   {  
      %initialRot = $mmSelectedShape.getNodeRot(%seq,0,0);//returns frame zero
      %deltaX = 0;//-1 * getWord(%initialRot,0); //See how this works...
      %deltaY = 0;//-1 * getWord(%initialRot,1);
      %deltaZ = getWord(%initialRot,2);
      %deltaRot = %deltaX @ " " @ %deltaY @ " " @ %deltaZ;
      $mmSelectedShape.adjustNodeRotRegion(%seq,0,%deltaRot,0.0,1.0);
   }
}

function mmMoveForward()
{
   if (!isObject($mmSelectedShape))
      return;
      
   %seq = $mmSequenceList.getSelected();
   if (%seq > -1)
   {
      %startPos = $mmSelectedShape.getNodeTrans(%seq,0);
      %finalPos = $mmSelectedShape.getNodeTrans(%seq,$mmSelectedShape.getSeqNumKeyframes(%seq)-1);
      %diff = VectorNormalize(VectorSub(%finalPos,%startPos));

      %forward = "0 1 0";
      %eulerArc = "0 0 0";
      if (VectorDot(%diff,%forward) < -0.999)//(ie, within small tolerance of 180 degrees opposite)
         %eulerArc = "0 0 180";
      else
      {
         //HERE: find the Z rotation!
         %eulerArc = rotationArcDegrees(%diff,%forward);//Actually, all rotations...
         echo("anim diff: " @ %diff @ ", euler arc: " @ %eulerArc);         
      }
      //%deltaRot = "0 0 " @ %deltaZ;
      $mmSelectedShape.adjustNodeRotRegion(%seq,0,%eulerArc,0.0,1.0); 
   }
}


function mmGroundCaptureSeq(%this)
{
   if (!isObject($mmSelectedShape))
      return;
      
   %seq = $mmSequenceList.getSelected();
   if ($mmSelectedShape.getSeqFrames(%seqnum) == $mmSelectedShape.getSeqGroundFrames(%seq))
   {
      $mmSelectedShape.unGroundCaptureSeq(%seq); 
      //GroundAnimateCheckbox.visible = false;
      $mmGroundCaptureButton.setText("Ground Capture");
      //SequenceNumGroundframes.setText($actor.getSeqNumGroundFrames(%seq));
   } else {
      $mmSelectedShape.groundCaptureSeq(%seq);
      //GroundAnimateCheckbox.visible = true;
      $mmGroundCaptureButton.setText("Un Ground Capture");
      //SequenceNumGroundframes.setText($actor.getSeqNumGroundFrames(%seq));
   }
   //EcstasyToolsWindow::updateSeqForm(%this);
}

function mmSequenceBlendToggle()
{
   %seq = $mmSequenceList.getSelected();
   if ((!isObject($mmSelectedShape))||(!(%seq>=0)))
      return;
      
   $mmSelectedShape.setSeqBlend(%seq,$mmSequenceBlend);
}

function mmSequenceCyclicToggle()
{
   %seq = $mmSequenceList.getSelected();
   if ((!isObject($mmSelectedShape))||(!(%seq>=0)))
      return;
   
   $mmSelectedShape.setSeqCyclic(%seq,$mmSequenceCyclic);
}

function mmSequenceGroundAnimateToggle()
{
   %seq = $mmSequenceList.getSelected();
   if ((!isObject($mmSelectedShape))||(!(%seq>=0)))
      return;
   
}




/////////////////////////////////////////////////////////
//BVH Tab
function mmImportBvhSequence()
{
   if (!isObject($mmSelectedShape))
      return;
      
   if (strlen($Pref::BvhDir))
      %openFileName = mmGetOpenFilename($Pref::BvhDir,"bvh");
   else
      %openFileName = mmGetOpenFilename($mmSelectedShape.getPath(),"bvh");
        
   
   $mmSelectedShape.importBvh(false,%openFileName,$mmBvhImportProfileList.getText(),false);
   
}

function mmImportBvhDirectory()
{
   
}

function mmBvhImportScene()
{
   
}

function mmExportBvhSequence()
{
   if (strlen($Pref::BvhDir))
      %saveFileName = mmGetSaveFilename($Pref::BvhDir,"bvh");
   else
      %saveFileName = mmGetSaveFilename($mmSelectedShape.getPath(),"bvh");
       
   $mmSelectedShape.saveBvh(false,%saveFileName,$mmBvhExportProfileList.getText(),false);
   
}

function mmExportBvhDirectory()
{
   
}

function mmBvhExportScene()
{
   
}

function mmAddBvhProfile()
{   
   if (!isObject($mmSelectedShape))
      return;
         
   if (strlen($Pref::BvhDir))
      %openFileName = mmGetOpenFilename($Pref::BvhDir,"bvh");
   else
      %openFileName = mmGetOpenFilename($mmSelectedShape.getPath(),"bvh");
   
   %profileName = "test";//Whoops, need an add bvh profile form now, to get this
   //as well as provide an option for supplying scale, etc. Try this to test 
   //import function though.
   
   $mmSelectedShape.importBvhSkeleton(%openFileName,%profileName);
       
   return;
}

function mmDeleteBvhProfile()
{
   
}

function mmSelectBvhProfile()
{
   if (!isObject($mmSelectedShape))
      return;
      
   $mmBvhBvhNodeList.clear();
   $mmBvhLinkedNodesList.clear();
   
   %profile_id = $mmBvhProfileList.getSelected();
   if (%profile_id<=0)
      return;
      
   %query = "SELECT id,name FROM bvhProfileNode WHERE profile_id=" @ %profile_id @ ";";
   %resultSet = sqlite.query(%query,0);
   if (%resultSet)
   {
      while (!sqlite.endOfResult(%resultSet))
      {
         %id = sqlite.getColumn(%resultSet, "id");
         %name = sqlite.getColumn(%resultSet, "name");            
         $mmBvhBvhNodeList.add(%name,%id);
         sqlite.nextRow(%resultSet);         
      }
      sqlite.clearResult(%resultSet);
   }   
   
   %query = "SELECT id FROM bvhProfileSkeleton WHERE profile_id=" @ %profile_id @ 
   " AND skeleton_id=" @ $mmSelectedShape.skeletonID @ ";";
   %resultSet = sqlite.query(%query,0);
   %id = sqlite.getColumn(%resultSet, "id");
   sqlite.clearResult(%resultSet);
   
   if (%id>0)
   {      
      %query = "SELECT id,bvhNodeName,skeletonNodeName FROM bvhProfileSkeletonNode " @ 
               "WHERE bvhProfileSkeleton_id=" @ %id @ ";";
      %resultSet = sqlite.query(%query,0);
      while (!sqlite.endOfResult(%resultSet))
      {
         %id = sqlite.getColumn(%resultSet, "id");
         %bvhName = sqlite.getColumn(%resultSet, "bvhNodeName");            
         %skelName = sqlite.getColumn(%resultSet, "skeletonNodeName");   
         %text = %skelName @ " - " @ %bvhName;         
         $mmBvhLinkedNodesList.add(%text,%id);
         sqlite.nextRow(%resultSet);
      }
      sqlite.clearResult(%resultSet);
   }   
}

function mmBvhLinkNode()
{
   if (!isObject($mmSelectedShape))
      return;
      
   %node_id = 0;
   %skeleton_id = $mmSelectedShape.skeletonID;
   if ((%skeleton_id<=0)||(numericTest(%skeleton_id)==false))
      return;
   
   %query = "SELECT id FROM bvhProfileSkeleton WHERE profile_id=" @
             $mmBvhProfileList.getSelected() @ " AND skeleton_id = " @ %skeleton_id @";";
   %resultSet = sqlite.query(%query, 0); 
   if (sqlite.numRows(%resultSet)==1)
   {
      %bvhProfileSkeleton_id = sqlite.getColumn(%resultSet, "id");
      sqlite.clearResult(%resultSet);
   }
   else 
   {
      echo("ERROR: there are " @ sqlite.numRows(%resultSet) @ " bvhProfileSkeletons for bvh " @
         $mmBvhProfileList.getSelected() @ " and skeleton " @ %skeleton_id @ ", should be one.");
      sqlite.clearResult(%resultSet);      
      return;
   }
   %query = "INSERT INTO bvhProfileSkeletonNode (bvhProfileSkeleton_id,bvhNodeName,skeletonNodeName) " @ 
            "VALUES (" @ %bvhProfileSkeleton_id @ ",'" @ $mmBvhBvhNodeList.getText() @ "','" @
             $mmBvhModelNodeList.getText() @ "');"; 
   sqlite.query(%query, 0); 
   echo("Inserted new bvhProfileNode! " @ $mmBvhBvhNodeList.getText());
   /*
   //Maybe do some error checking at some point, at least enough to remind them to delete existing
   //links, if we don't do it automatically.
   
   %query = "SELECT id FROM bvhProfileSkeletonNode WHERE bvhProfileSkeleton_id=" @
            %bvhProfileSkeleton_id @ " AND bvhNodeName = '" @ $mmBvhBvhNodeList.getText() @ "';";
   %resultSet = sqlite.query(%query, 0); 
   
   %query = "UPDATE bvhProfileSkeletonNode SET skeletonNodeName='" @ 
            $mmBvhModelNodeList.getText() @ "' WHERE bvhProfileSkeleton_id=" @ 
            %bvhProfileSkeleton_id @ " AND bvhNodeName = '" @ 
            $mmBvhBvhNodeList.getText() @ "';";
   sqlite.query(%query, 0); 
   */
   
   
   //EcstasyToolsWindow::refreshBvhNodesList();
}

function mmBvhUnlinkNode()
{
   if (!isObject($mmSelectedShape))
      return;
      
   %skeleton_id = $mmSelectedShape.skeletonID;
   if ((%skeleton_id<=0)||(numericTest(%skeleton_id)==false))
      return;
   
   %query = "SELECT id FROM bvhProfileSkeleton WHERE profile_id=" @
             $mmBvhProfileList.getSelected() @ " AND skeleton_id = " @ %skeleton_id @";";
   %resultSet = sqlite.query(%query, 0); 
   if (sqlite.numRows(%result)==1)
   {
      %bvhProfileSkeleton_id = sqlite.getColumn(%result, "id");
      sqlite.clearResult(%resultSet);
   }
   else 
   {
      echo("ERROR: there are " @ sqlite.numRows(%result) @ " bvhProfileSkeletons for bvh " @
         $mmBvhProfileList.getSelected() @ " and skeleton " @ %skeleton_id @ 
         ", should be one.");
      return;
   }

   %query = "DELETE FROM bvhProfileSkeletonNode WHERE bvhProfileSkeleton_id=" @
            %bvhProfileSkeleton_id @ " AND bvhNodeName='" @ $mmBvhBvhNodeList.getText() @
            "' AND skeletonNodeName='" @ $mmBvhModelNodeList.getText() @ "';";
   sqlite.query(%query,0);
   
   //EcstasyToolsWindow::refreshBvhNodesList();   
}

/*


function EcstasyToolsWindow::unlinkBvhNode()
{
   //if(!EcstasyToolsWindow::StartSQL())
      //return;
   if (!$actor)
      return;
   %skeleton_id = $actor.getSkeletonId();
   if (numericTest(%skeleton_id)==false) %skeleton_id = 0;
   
   %query = "SELECT id FROM bvhProfileSkeleton WHERE bvhProfile_id=" @
             BvhImportProfilesList.getSelected() @ " AND skeleton_id = " @ %skeleton_id @";";
   %result = sqlite.query(%query, 0); 
   if (sqlite.numRows(%result)==1)
      %bvhProfileSkeleton_id = sqlite.getColumn(%result, "id");
      if (numericTest(%bvhProfileSkeleton_id)==false) %bvhProfileSkeleton_id = 0;
   else 
      echo("ERROR: there are " @ sqlite.numRows(%result) @ " bvhProfileSkeletons for bvh " @
         BvhImportProfilesList.getSelected() @ " and skeleton " @ %skeleton_id @ 
         ", should be one.");

   %query = "UPDATE bvhProfileSkeletonNode SET skeletonNodeName='' " @ 
            " WHERE bvhProfileSkeleton_id=" @ 
            %bvhProfileSkeleton_id @ " AND bvhNodeName = '" @ 
            BvhNodesList.getText() @ "';";
   %result = sqlite.query(%query, 0); 
   
   sqlite.clearResult(%result);
   //EcstasyToolsWindow::CloseSQL();
   
   EcstasyToolsWindow::refreshBvhNodesList();
}
*/

function mmSelectBvhLinkedNode()
{
   %panel = $mmBvhTab.findObjectByInternalName("bvhPanel");
   
   %posRotAX = %panel.findObjectByInternalName("bvhPoseRotAX");
   %posRotAY = %panel.findObjectByInternalName("bvhPoseRotAY");
   %posRotAZ = %panel.findObjectByInternalName("bvhPoseRotAZ");
   %posRotBX = %panel.findObjectByInternalName("bvhPoseRotBX");
   %posRotBY = %panel.findObjectByInternalName("bvhPoseRotBY");
   %posRotBZ = %panel.findObjectByInternalName("bvhPoseRotBZ");
   %fixRotAX = %panel.findObjectByInternalName("bvhFixRotAX");
   %fixRotAY = %panel.findObjectByInternalName("bvhFixRotAY");
   %fixRotAZ = %panel.findObjectByInternalName("bvhFixRotAZ");
   %fixRotBX = %panel.findObjectByInternalName("bvhFixRotBX");
   %fixRotBY = %panel.findObjectByInternalName("bvhFixRotBY");
   %fixRotBZ = %panel.findObjectByInternalName("bvhFixRotBZ");
   
   %id = $mmBvhLinkedNodesList.getSelected();
   
   %query = "SELECT * FROM bvhProfileSkeletonNode WHERE id=" @ %id @ ";";
   %resultSet = sqlite.query(%query,0);
   if (sqlite.numRows(%resultSet)==1)
   {
      %posRotAX.setText(sqlite.getColumn(%resultSet, "poseRotA_x"));
      %posRotAY.setText(sqlite.getColumn(%resultSet, "poseRotA_y"));
      %posRotAZ.setText(sqlite.getColumn(%resultSet, "poseRotA_z"));
      %posRotBX.setText(sqlite.getColumn(%resultSet, "poseRotB_x"));
      %posRotBY.setText(sqlite.getColumn(%resultSet, "poseRotB_y"));
      %posRotBZ.setText(sqlite.getColumn(%resultSet, "poseRotB_z"));
      %fixRotAX.setText(sqlite.getColumn(%resultSet, "fixRotA_x"));
      %fixRotAY.setText(sqlite.getColumn(%resultSet, "fixRotA_y"));
      %fixRotAZ.setText(sqlite.getColumn(%resultSet, "fixRotA_z"));
      %fixRotBX.setText(sqlite.getColumn(%resultSet, "fixRotB_x"));
      %fixRotBY.setText(sqlite.getColumn(%resultSet, "fixRotB_y"));
      %fixRotBZ.setText(sqlite.getColumn(%resultSet, "fixRotB_z"));
   }
   sqlite.clearResult(%resultSet);
   
   
   
}


//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
//  Sequence Timeline Window     /////////////////////////////////////////////////

function setupMegaMotionSequenceWindow()
{   
   if (!isDefined("MegaMotionSequenceWindow"))
      return;   
      
   $mmSequenceSlider = MegaMotionSequenceWindow.findObjectByInternalName("sequenceSlider");
   $mmSequenceSlider.ticks = 0;
   
   $mmSequenceSliderIn = MegaMotionSequenceWindow.findObjectByInternalName("sequenceInBar");
   $mmSequenceSliderOut = MegaMotionSequenceWindow.findObjectByInternalName("sequenceOutBar");
   
   $mmSequenceInFrame = MegaMotionSequenceWindow.findObjectByInternalName("sequenceInFrame");
   $mmSequenceOutFrame = MegaMotionSequenceWindow.findObjectByInternalName("sequenceOutFrame");
   $mmSequenceFindLoopDelta = MegaMotionSequenceWindow.findObjectByInternalName("sequenceFindLoopDelta");
   
   $mmSequenceInFrame.setText("0");
   $mmSequenceOutFrame.setText("1");
   $mmSequenceFindLoopDelta.setText("0");
   
   $mmSequenceFrame = MegaMotionSequenceWindow.findObjectByInternalName("sequenceFrame");
   $mmSequenceInFrame.setText("0");
}

function mmSequenceSetInBar()
{
   %outPos = $mmSequenceSliderOut.getPosition();
   
   %frame = $mmSequenceSlider.value;
   %frame = mFloor(%frame);
   
   $mmSelectedShape.getClientObject().recordCropStartPositions();//Used for loop detecting.
   
   %numFrames = $mmSequenceSlider.range.y;
   %newPos = $mmSequenceSlider.getPosition();
   %newPos.x += (%frame / %numFrames) * ($mmSequenceSlider.extent.x - 12);//Slider can't go to end.
   %newPos.y -= 12;
   
   if (%outPos.x < %newPos.x)
   {
      $mmSequenceSliderOut.setPosition(%newPos.x,%newPos.y);
      $mmSequenceOutFrame.setText(%frame);
      $mmSequenceSliderIn.setPosition(%outPos.x,%outPos.y);
      %sliderPos = $mmSequenceSlider.getPosition().x;
      %frame = mCeil(((%outPos.x - %sliderPos)/$mmSequenceSlider.extent.x)*$mmSequenceSlider.range.y);
      $mmSequenceInFrame.setText(%frame);
   }
   else if (%outPos.x == %newPos.x)
   {      
      $mmSequenceSliderIn.setPosition(%newPos.x,%newPos.y);
      $mmSequenceInFrame.setText(%frame);
      %frame += 1;
      %newPos = $mmSequenceSlider.getPosition();
      %newPos.x += (%frame / %numFrames) * ($mmSequenceSlider.extent.x - 12);
      %newPos.y -= 12;
      $mmSequenceSliderOut.setPosition(%newPos.x,%newPos.y);
      $mmSequenceOutFrame.setText(%frame);
   }
   else
   {
      $mmSequenceSliderIn.setPosition(%newPos.x,%newPos.y);
      $mmSequenceInFrame.setText(%frame);
   }      
}

function mmSequenceBackwardToIn()
{
   $mmSelectedShape.pauseSeq();
   
   %inPos = $mmSequenceSliderIn.getPosition().x;
   %sliderPos = $mmSequenceSlider.getPosition().x;
   
   //Marker position as percentage of total slider width, -12 because marker can't get to the end.
   %inSeqPos = ((%inPos - %sliderPos)/($mmSequenceSlider.extent.x - 12));
   $mmSelectedShape.setSeqPos(%inSeqPos); 
}

function mmSequenceStepBackward()
{
   $mmSelectedShape.pauseSeq();
   
   //%seqFrameStep = 1.0/$mmSelectedShape.getSeqFrames($mmSelectedShape.getAmbientSeq());
   %seqFrameStep = 1.0/$mmSelectedShape.getSeqFrames($mmSequenceList.getSelected());
   //%seqFrameStep = 1.0/getWord($mmSequenceSlider.getRange(),1);//Hm, NOPE.
   %pos = $mmSelectedShape.getSeqPos();
   %newPos = %pos - %seqFrameStep;
   if (%newPos < 0.0)
      %newPos = 0.0;
      
   $mmSelectedShape.setSeqPos(%newPos);  
}

function mmSequencePlayBackward()
{
   %pos = $mmSelectedShape.getSeqPos();
   if (%pos==0) %pos = 1.0;
   //$mmSelectedShape.playSeqByNum($mmSelectedShape.getAmbientSeq());
   $mmSelectedShape.playSeqByNum($mmSequenceList.getSelected());
   $mmSelectedShape.reverseSeq();
   $mmSelectedShape.setSeqPos(%pos);   
}

function mmSequencePause()
{ 
   $mmSelectedShape.pauseSeq();
}

function mmSequencePlayForward()
{
   %pos = $mmSelectedShape.getSeqPos();
   //$mmSelectedShape.playSeqByNum($mmSelectedShape.getAmbientSeq());
   $mmSelectedShape.playSeqByNum($mmSequenceList.getSelected());
   $mmSelectedShape.forwardSeq();
   $mmSelectedShape.setSeqPos(%pos);
}

function mmSequenceStepForward()
{
   $mmSelectedShape.pauseSeq();
   
   //%seqFrameStep = 1.0/$mmSelectedShape.getSeqFrames($mmSelectedShape.getAmbientSeq());
   %seqFrameStep = 1.0/$mmSelectedShape.getSeqFrames($mmSequenceList.getSelected());
   %pos = $mmSelectedShape.getSeqPos();
   %newPos = %pos + %seqFrameStep;
   if (%newPos > 1.0)
      %newPos = 1.0;
      
   $mmSelectedShape.setSeqPos(%newPos); 
}

function mmSequenceForwardToOut()
{
   $mmSelectedShape.pauseSeq();
   
   %outPos = $mmSequenceSliderOut.getPosition().x;
   %sliderPos = $mmSequenceSlider.getPosition().x;
   
   //Marker position as percentage of total slider width.
   %outSeqPos = ((%outPos - %sliderPos)/($mmSequenceSlider.extent.x - 12));
   $mmSelectedShape.setSeqPos(%outSeqPos); 
}


function mmSequenceSetOutBar()
{   
   %inPos = $mmSequenceSliderIn.getPosition();
   
   %frame = $mmSequenceSlider.value;
   %frame = mCeil(%frame);
   %numFrames = $mmSequenceSlider.range.y;
   %newPos = $mmSequenceSlider.getPosition();
   %newPos.x += (%frame / %numFrames) * ($mmSequenceSlider.extent.x - 12);
   %newPos.y -= 12;
   
   if (%inPos.x > %newPos.x)
   {
      $mmSequenceSliderIn.setPosition(%newPos.x,%newPos.y);
      $mmSequenceInFrame.setText(%frame);
      $mmSequenceSliderOut.setPosition(%inPos.x,%inPos.y);
      %sliderPos = $mmSequenceSlider.getPosition().x;
      %frame = mCeil(((%inPos.x - %sliderPos)/$mmSequenceSlider.extent.x)*$mmSequenceSlider.range.y);
      $mmSequenceOutFrame.setText(%frame);
   }
   else if (%inPos.x == %newPos.x)
   {      
      $mmSequenceSliderIn.setPosition(%newPos.x,%newPos.y);
      $mmSequenceInFrame.setText(%frame);
      %frame += 1;
      %newPos = $mmSequenceSlider.getPosition();
      %newPos.x += (%frame / %numFrames) * ($mmSequenceSlider.extent.x - 12);
      %newPos.y -= 12;
      $mmSequenceSliderOut.setPosition(%newPos.x,%newPos.y);
      $mmSequenceOutFrame.setText(%frame);
   }
   else
   {
      $mmSequenceSliderOut.setPosition(%newPos.x,%newPos.y);
      $mmSequenceOutFrame.setText(%frame);
   }
   //echo("out bar, frame " @ %frame @ " numFrames " @ %numFrames @ " pos " @
   //       %newPos.x @ " startpos " @ $mmSequenceSlider.getPosition().x @ " extent " 
   //       @ $mmSequenceSlider.extent.x);

}

function mmSequenceSliderClick()
{
   $mmSelectedShape.pauseSeq();
   
   %value = $mmSequenceSlider.value; 
   %newPos = %value / $mmSequenceSlider.range.y;   
   $mmSelectedShape.setSeqPos(%newPos); 
}

function mmSequenceSliderDrag()
{
   $mmSelectedShape.pauseSeq();
      
   %value = $mmSequenceSlider.value; 
   %newPos = %value / $mmSequenceSlider.range.y;   
   $mmSelectedShape.setSeqPos(%newPos); 
}

function mmRefreshSequenceList()
{
   $mmSequenceList.clear();
   %numSeqs = $mmSelectedShape.getNumSeqs();
   for (%j=0;%j<%numSeqs;%j++)
   {
      %name = $mmSelectedShape.getSeqName(%j);
      $mmSequenceList.add(%name,%j);         
   }
}

function mmCrop()
{
   if (!isObject($mmSelectedShape))
      return;

   if (strlen($Pref::DsqDir))
      %saveFileName = mmGetSaveFileName($Pref::DsqDir,"dsq");
   else
      %saveFileName = mmGetSaveFileName($actor.getPath(),"dsq");

   %crop_start = $mmSequenceInFrame.getText()/$mmSequenceSlider.range.y;
   %crop_stop = $mmSequenceOutFrame.getText()/$mmSequenceSlider.range.y;
  
   //HERE: crop from crop_start to crop_stop, into a new sequence, append it to list.
   %seqnum = $mmSelectedShape.getSeqNum($mmSequenceList.getText());
   
   $mmSelectedShape.cropSequence(%seqnum,%crop_start,%crop_stop ,%saveFileName);
 
   $mmSelectedShape.dropSequence($mmSelectedShape.getNumSeqs()-1);   
   $mmSelectedShape.loadSequence(%saveFileName);//maybe?
   
   //$mmSequenceSliderIn.setPosition();//Need a function for these, isolate the logic.
   //$mmSequenceSliderOut.setPosition();
   
   mmRefreshSequenceList();
   
   //$mmSequenceList.setSelected($mmSequenceList.size()-1);
   //EcstasyToolsWindow::selectSequence();
}

function mmFindLoop()
{
   $mmRotDeltaSumMin = 999.0;
   $mmRotDeltaSumDescending = 0;
   $mmRotDeltaSumLast = 0;
   $mmLoopDetecting = 1;
   EcstasySequenceSlider.paused = false;
   $mmSelectedShape.forwardSeq();
   echo("Find Loop! deltaSumMin " @ $mmRotDeltaSumMin);
   //$actor.startAnimatingAtPos(SequencesList.getText(),EcstasySequenceSlider.value);
}

function mmSmoothLoop()
{
   if (!isObject($mmSelectedShape))
      return;
   
   %seq = $mmSequenceList.getSelected();
   $mmSelectedShape.smoothLoopTransition(%seq,$mmLoopDetectorSmooth);
}

////////////////////////////////////////////////////
function mmGetOpenFileName(%defaultFilePath,%type)
{
   if (%type$="dts")
      %filter = "DTS Files (*.dts)|*.dts|Collada Files (*.dae)|*.dae|FBX Files (*.fbx)|*.fbx|All Files (*.*)|*.*|";
   else if (%type$="dsq")
      %filter = "DSQ Files (*.dsq)|*.dsq|All Files (*.*)|*.*|";
   else if (%type$="bvh")
      %filter = "BVH Files (*.bvh)|*.bvh|All Files (*.*)|*.*|";    
   //else if ...
      
   %dlg = new OpenFileDialog()
   {
      Filters        = %filter;
      DefaultPath    = %defaultFileName;
      //DefaultFile    = %defaultFileName;
      ChangePath     = false;
      MustExist      = true;
   };
   if(%dlg.Execute())
   {
      $Pref::DsqDir = filePath( %dlg.FileName );
      %filename = %dlg.FileName;      
      %dlg.delete();
      return %filename;
   }
   %dlg.delete();
   return "";      
}

function mmGetSaveFileName(%defaultFilePath,%type)
{
   if (%type$="dts")
      %filter = "DTS Files (*.dts)|*.dts|All Files (*.*)|*.*|";
   else if (%type$="dsq")
      %filter = "DSQ Files (*.dsq)|*.dsq|All Files (*.*)|*.*|";
   else if (%type$="bvh")
      %filter = "BVH Files (*.bvh)|*.bvh|All Files (*.*)|*.*|";
   //else if ...
      
   %dlg = new SaveFileDialog()
   {
      Filters        = %filter;
      DefaultPath    = %defaultFilePath;
      ChangePath     = false;
      OverwritePrompt   = true;
   };
   if(%dlg.Execute())
   {
      $Pref::DsqDir = filePath( %dlg.FileName );
      %filename = %dlg.FileName;      
      %dlg.delete();
      return %filename;
   }
   %dlg.delete();
   return "";   
   
}

//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////

function MegaMotionTick()
{  
   //General function for anything in MegaMotion that needs a tick but doesn't do it itself.
   
   //Sequence slider needs to keep up with selected shape's animation:
   if ((isObject($mmSelectedShape))&&($mmSequenceSlider)&&(MegaMotionSequenceWindow.isVisible()))
   {
      %threadPos = $mmSelectedShape.getSeqPos();
      %range = $mmSequenceSlider.range;
      %numFrames = %range.y;//range is a Point2F, so begin and end are x and y.
      %frame = %threadPos * %numFrames;
      %frame = mRound(%frame * 100)/100;//(I don't think we have a "round to n decimals" in torque script yet?)
      $mmSequenceSlider.setValue(%frame);      
      $mmSequenceFrame.setText(%frame);
      
      if ($mmLoopDetecting)
      {
         //TimelineRotDeltaSum.visible = 1;
         //cropStopCyclicButton.visible = 1;
         %seq = $mmSequenceList.getSelected();
         %current_frame = mFloor(%threadpos * $mmSequenceSlider.range.y);
         %start_frame = $mmSequenceInFrame.getText();
         //if (%start_frame==%current_frame)    
         //{//Somehow these starting values are getting lost between mmFindLoop and here. (??)
         //   $mmRotDeltaSumMin = 999.0;
         //   $mmRotDeltaSumDescending = 0;
         //   $mmRotDeltaSumLast = 0;
         //}
         
         %seqDeltaSum = $mmSelectedShape.getClientObject().getSeqDeltaSum(%seq,%current_frame,%start_frame);
         $mmSequenceFindLoopDelta.setText(%seqDeltaSum);
         //TimelineRotDeltaSum.setText(mFloatLength(%seqDeltaSum,3));
         %frame_from_start = %current_frame-%start_frame;
         echo(" frame: " @ %current_frame @ ", deltaSum " @ %seqDeltaSum @ "  last " @ $mmRotDeltaSumLast @ 
            ", deltaSumMin " @ $mmRotDeltaSumMin @ " isDescending " @ $mmRotDeltaSumDescending);
         if ((%seqDeltaSum < $mmRotDeltaSumLast)&&(%frame_from_start > $mmLoopDetectorDelay))
            $mmRotDeltaSumDescending = 1;
         else 
         {
            if ($mmRotDeltaSumDescending)
            {
               if ($mmRotDeltaSumLast < $mmRotDeltaSumMin)
               {
                  echo("loop detector found out pos: " @ %current_frame @ " frame-from-start " @ %frame_from_start );
                  $mmRotDeltaSumMin = $mmRotDeltaSumLast;
                  $mmRotDeltaSumLastFrame = %current_frame-1;                  
                  //SequencesCropStopKeyframeText.setText($rotDeltaSumLastFrame);
                  %markOutPos = mFloatLength($mmRotDeltaSumLastFrame / $mmSelectedShape.getSeqFrames(%seq),3);
                  $mmSequenceOutFrame.setText(%markOutPos);
                  $mmSequenceSlider.setValue($mmRotDeltaSumLastFrame);
                  mmSequenceSetOutBar();
                  echo("found a minimum: " @ $mmRotDeltaSumMin @ ", frame " @ $mmRotDeltaSumLastFrame);
               }
               $mmRotDeltaSumDescending = 0;
            }
         }
         $mmRotDeltaSumLast = %seqDeltaSum;
         if (((%current_frame==0)&&($mmRotDeltaSumMin<999.0))||(%frame_from_start > $mmLoopDetectorMax))
         {
            echo("ending loop detection: current frame " @ %current_frame @ " frame-from-start " @ %frame_from_start @ 
               ", loop detector max: " @ $loopDetectorMax);
            //SequencesCropStopKeyframeText.setText($rotDeltaSumLastFrame);
            %markOutPos = mFloatLength($mmRotDeltaSumLastFrame / $mmSelectedShape.getSeqFrames(%seq),3);
            //SequencesCropStopText.setText(%markOutPos);
            //$crop_stop = %markOutPos;//Note: this sucks, we should just check SequencesCropStopText.getText instead.
            //TimelineRotDeltaSum.setText(mFloatLength($rotDeltaSumMin,3));
            $mmSequenceOutFrame.setText(%markOutPos);            
            $mmLoopDetecting = 0;
            $mmSequenceSlider.setValue($mmSequenceOutFrame.getText());
            mmSequenceSetOutBar();
            //EcstasySequenceSlider::setSliderToPos(SequencesCropStartText.getText());
            //$showRotDeltaSum = 0;
         }          
      }
   }
   
   schedule(30,0,"MegaMotionTick");//30 MS =~ 32 times per second.
}

function startRecording()
{
   for (%i=0;%i<SceneShapes.getCount();%i++)
   {
      %shape = SceneShapes.getObject(%i);  
      %shape.setIsRecording(true);
   }   
}

function stopRecording()
{
   for (%i=0;%i<SceneShapes.getCount();%i++)
   {
      %shape = SceneShapes.getObject(%i);  
      %shape.setIsRecording(false);
   }   
}

function makeSequences()
{
   //OKAY... here we go. We now need to:
   // a) find our model's home directory   
   // b) in that directory, create a new directory with a naming protocol
   //       "scene_[%scene_id].[timestamp]"?
   // c) fill it with sequences
   
   //For now, just "workSeqs", if name changes we'll have to update M4.cs every time.
   %dirPath = %shape.getPath() @ "/scenes";
   createDirectory(%dirPath);//make shape/scenes folder first, if necessary.
   %dirPath = %shape.getPath() @ "/scenes/" @ %shape.sceneID ;//then make specific scene folder.
   for (%i=0;%i<SceneShapes.getCount();%i++)
   {
      %shape = SceneShapes.getObject(%i);  
      %dirPath = %shape.getPath() @ "/scenes/" @ %shape.sceneID ;
      %shape.makeSequence(%dirPath @ "/" @ %shape.getSceneShapeID());
   }
}



//////////////////////////////////////////////////////////////////////

function shapesAct()
{
   //pdd(1);
   for (%i=0;%i<SceneShapes.getCount();%i++)
   {
      %shape = SceneShapes.getObject(%i); 
      
      //%clientShape = %shape.getClientObject();//SINGLE PLAYER
      //%clientShape.createVehicle(%clientShape.getPosition(),0);//getRotation?   
      //echo("adding vehicle for sceneShape " @ %clientShape @ " position " @ %clientShape.getPosition() );
      
      //%shape.setHasGravity(false);
      
      //%shape.setDynamic(1);
      
      //%shape.setPartDynamic(0,0);
      //%shape.setPartDynamic(1,0);
      //%shape.setPartDynamic(2,0);  
      //%shape.setPartDynamic(3,0); 
      //%shape.setPartDynamic(4,0); 
      
      //%shape.setPartDynamic(5,0); 
      //%shape.setPartDynamic(6,0); 
      //%shape.setPartDynamic(7,0);
      //%shape.setPartDynamic(8,0);
      
      //%shape.setPartDynamic(9,0); 
      //%shape.setPartDynamic(10,0); 
      //%shape.setPartDynamic(11,0);
      //%shape.setPartDynamic(12,0);
       
      //%shape.setPartDynamic(13,0);
      //%shape.setPartDynamic(14,1);
      //%shape.setPartDynamic(15,1);
      
      //%shape.setPartDynamic(16,0);
      //%shape.setPartDynamic(17,0);
      //%shape.setPartDynamic(18,0);     
   } 
}









//////////////////////////////////////////////////////////////////////////////////////////////////////////////
//For each form that we hook up to a top menu, give it an "expose" function to load it and call setup for it.
//OBSOLETE, TESTING /////////////////////////////
/*
function exposeMegaMotion()
{
   if (isDefined("MegaMotionWindow"))
      MegaMotionWindow.delete();
   
   makeSqlGuiForm($MegaMotionFormID);
   setupMegaMotionForm();   
}

function setupMegaMotionForm()
{
   if (!isDefined("MegaMotionWindow"))
      return;   
      
   %sceneSetList = MegaMotionWindow.findObjectByInternalName("sceneSetList");
   %sceneList = MegaMotionWindow.findObjectByInternalName("sceneList");
   
   %sceneSetList.add("Testing","1");
   %sceneSetList.add("Portlingrad","2");
   %sceneSetList.setSelected(1);
}
*/


//function testSpatialite()
//{
//   //%query = "CREATE TABLE spatialTest ( id INTEGER, name TEXT NOT NULL, geom BLOB NOT NULL);";
//   %query = "INSERT INTO spatialTest ( id , name, geom ) VALUES (1,'Test01',GeomFromText('POINT(1 2)'));";
   
//   %result = sqlite.query(%query, 0);
	
//   if (%result)
//      echo("spatialite inserted into a table with a geom!");
//   else
//      echo("spatialite failed to insert into a table with a geom!  "  );
//}

//OBSOLETE, TESTING /////////////////////////////

//Nice try, but even with modifications to GuiWindowCtrl, there is no way to intercept a mouse
//event that lands on a control. Hence, we're screwed if we want to select controls this way.
//function MegaMotionWindow::onMouseDown(%this,%pos)
//{
   //echo("MegaMotionWindow onMouseDown!!!!  this.pos " @ %this.getPosition() @ " mouse pos " @ %pos);  
//}
//
//function MegaMotionWindow::onMouseUp(%this,%pos)
//{
   //echo("MegaMotionWindow onMouseUp!!!!  this.pos " @ %this.getPosition() @ " mouse pos " @ %pos);  
//}
